using System;
using System.Collections.Generic;

namespace DiffMatchPatch
{
    public interface IMatchOperations
    {
        int match_main(string text, string pattern, int loc);
    }
    
    public class MatchOperations : IMatchOperations
    {
        public float Match_Threshold = 0.5f;
        public int Match_Distance = 1000;
        protected short Match_MaxBits = 32;

        public int match_main(string text, string pattern, int loc) {
            // Check for null inputs not needed since null can't be passed in C#.

            loc = Math.Max(0, Math.Min(loc, text.Length));
            if (text == pattern) {
                // Shortcut (potentially not guaranteed by the algorithm)
                return 0;
            } else if (text.Length == 0) {
                // Nothing to match.
                return -1;
            } else if (loc + pattern.Length <= text.Length
                       && text.Substring(loc, pattern.Length) == pattern) {
                // Perfect match at the perfect spot!  (Includes case of null pattern)
                return loc;
            } else {
                // Do a fuzzy compare.
                return match_bitap(text, pattern, loc);
            }
        }

        protected int match_bitap(string text, string pattern, int loc) {
            // assert (Match_MaxBits == 0 || pattern.Length <= Match_MaxBits)
            //    : "Pattern too long for this application.";

            // Initialise the alphabet.
            Dictionary<char, int> s = match_alphabet(pattern);

            // Highest score beyond which we give up.
            double score_threshold = Match_Threshold;
            // Is there a nearby exact match? (speedup)
            int best_loc = text.IndexOf(pattern, loc, StringComparison.Ordinal);
            if (best_loc != -1) {
                score_threshold = Math.Min(match_bitapScore(0, best_loc, loc,
                    pattern), score_threshold);
                // What about in the other direction? (speedup)
                best_loc = text.LastIndexOf(pattern,
                    Math.Min(loc + pattern.Length, text.Length),
                    StringComparison.Ordinal);
                if (best_loc != -1) {
                    score_threshold = Math.Min(match_bitapScore(0, best_loc, loc,
                        pattern), score_threshold);
                }
            }

            // Initialise the bit arrays.
            int matchmask = 1 << (pattern.Length - 1);
            best_loc = -1;

            int bin_min, bin_mid;
            int bin_max = pattern.Length + text.Length;
            // Empty initialization added to appease C# compiler.
            int[] last_rd = new int[0];
            for (int d = 0; d < pattern.Length; d++) {
                // Scan for the best match; each iteration allows for one more error.
                // Run a binary search to determine how far from 'loc' we can stray at
                // this error level.
                bin_min = 0;
                bin_mid = bin_max;
                while (bin_min < bin_mid) {
                    if (match_bitapScore(d, loc + bin_mid, loc, pattern)
                        <= score_threshold) {
                        bin_min = bin_mid;
                    } else {
                        bin_max = bin_mid;
                    }
                    bin_mid = (bin_max - bin_min) / 2 + bin_min;
                }
                // Use the result from this iteration as the maximum for the next.
                bin_max = bin_mid;
                int start = Math.Max(1, loc - bin_mid + 1);
                int finish = Math.Min(loc + bin_mid, text.Length) + pattern.Length;

                int[] rd = new int[finish + 2];
                rd[finish + 1] = (1 << d) - 1;
                for (int j = finish; j >= start; j--) {
                    int charMatch;
                    if (text.Length <= j - 1 || !s.ContainsKey(text[j - 1])) {
                        // Out of range.
                        charMatch = 0;
                    } else {
                        charMatch = s[text[j - 1]];
                    }
                    if (d == 0) {
                        // First pass: exact match.
                        rd[j] = ((rd[j + 1] << 1) | 1) & charMatch;
                    } else {
                        // Subsequent passes: fuzzy match.
                        rd[j] = ((rd[j + 1] << 1) | 1) & charMatch
                                | (((last_rd[j + 1] | last_rd[j]) << 1) | 1) | last_rd[j + 1];
                    }
                    if ((rd[j] & matchmask) != 0) {
                        double score = match_bitapScore(d, j - 1, loc, pattern);
                        // This match will almost certainly be better than any existing
                        // match.  But check anyway.
                        if (score <= score_threshold) {
                            // Told you so.
                            score_threshold = score;
                            best_loc = j - 1;
                            if (best_loc > loc) {
                                // When passing loc, don't exceed our current distance from loc.
                                start = Math.Max(1, 2 * loc - best_loc);
                            } else {
                                // Already passed loc, downhill from here on in.
                                break;
                            }
                        }
                    }
                }
                if (match_bitapScore(d + 1, loc, loc, pattern) > score_threshold) {
                    // No hope for a (better) match at greater error levels.
                    break;
                }
                last_rd = rd;
            }
            return best_loc;
        }

        private double match_bitapScore(int e, int x, int loc, string pattern) {
            float accuracy = (float)e / pattern.Length;
            int proximity = Math.Abs(loc - x);
            if (Match_Distance == 0) {
                // Dodge divide by zero error.
                return proximity == 0 ? accuracy : 1.0;
            }
            return accuracy + (proximity / (float) Match_Distance);
        }

        protected Dictionary<char, int> match_alphabet(string pattern) {
            Dictionary<char, int> s = new Dictionary<char, int>();
            char[] char_pattern = pattern.ToCharArray();
            foreach (char c in char_pattern) {
                if (!s.ContainsKey(c)) {
                    s.Add(c, 0);
                }
            }
            int i = 0;
            foreach (char c in char_pattern) {
                int value = s[c] | (1 << (pattern.Length - i - 1));
                s[c] = value;
                i++;
            }
            return s;
        }
    }
}