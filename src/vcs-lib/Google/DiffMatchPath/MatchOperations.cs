﻿/*
 * Copyright 2008 Google Inc. All Rights Reserved.
 * Author: fraser@google.com (Neil Fraser)
 * Author: anteru@developer.shelter13.net (Matthaeus G. Chajdas)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Diff Match and Patch
 * http://code.google.com/p/google-diff-match-patch/
 */

/* Minor refactoring (code split) for better readability and to match C# naming style
*  Author: v.bliznikov@gmail.com
*/

using System;
using System.Collections.Generic;

namespace Google.DiffMatchPatch
{
    public class MatchOperations
    {
        /// <summary>
        /// At what point is no match declared (0.0 = perfection, 1.0 = very loose). 
        /// </summary>
        public float Match_Threshold = 0.5f;

        /// <summary>
        /// How far to search for a match (0 = exact location, 1000+ = broad match).
        /// </summary>
        /// <remarks>
        /// A match this many characters away from the expected location will add 1.0 to the score 
        /// (0.0 is a perfect match).
        /// </remarks>
        public int Match_Distance = 1000;

        // The number of bits in an int.
        public short Match_MaxBits = 32;

        /// <summary>
        /// Locate the best instance of 'pattern' in 'text' near 'loc'.
        /// Returns -1 if no match found.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="loc">The location to search around.</param>
        /// <returns>Best match index or -1.</returns>
        public int Match(string text, string pattern, int loc)
        {
            // Check for null inputs not needed since null can't be passed in C#.

            loc = Math.Max(0, Math.Min(loc, text.Length));
            if (text == pattern)
            {
                // Shortcut (potentially not guaranteed by the algorithm)
                return 0;
            }
            else if (text.Length == 0)
            {
                // Nothing to match.
                return -1;
            }
            else if (loc + pattern.Length <= text.Length
                     && text.Substring(loc, pattern.Length) == pattern)
            {
                // Perfect match at the perfect spot!  (Includes case of null pattern)
                return loc;
            }
            else
            {
                // Do a fuzzy compare.
                return MatchBitap(text, pattern, loc);
            }
        }

        /// <summary>
        /// Locate the best instance of 'pattern' in 'text' near 'loc' using the Bitap algorithm.  
        /// Returns -1 if no match found.
        /// </summary>
        /// <param name="text">The text to search.</param>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="loc">The location to search around.</param>
        /// <returns>Best match index or -1.</returns>
        protected int MatchBitap(string text, string pattern, int loc)
        {
            // assert (Match_MaxBits == 0 || pattern.Length <= Match_MaxBits)
            //    : "Pattern too long for this application.";

            // Initialise the alphabet.
            Dictionary<char, int> s = MatchAlphabet(pattern);

            // Highest score beyond which we give up.
            double score_threshold = Match_Threshold;
            // Is there a nearby exact match? (speedup)
            int best_loc = text.IndexOf(pattern, loc, StringComparison.Ordinal);
            if (best_loc != -1)
            {
                score_threshold = Math.Min(MatchBitapScore(0, best_loc, loc,
                    pattern), score_threshold);
                // What about in the other direction? (speedup)
                best_loc = text.LastIndexOf(pattern,
                    Math.Min(loc + pattern.Length, text.Length),
                    StringComparison.Ordinal);
                if (best_loc != -1)
                {
                    score_threshold = Math.Min(MatchBitapScore(0, best_loc, loc,
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
            for (int d = 0; d < pattern.Length; d++)
            {
                // Scan for the best match; each iteration allows for one more error.
                // Run a binary search to determine how far from 'loc' we can stray at
                // this error level.
                bin_min = 0;
                bin_mid = bin_max;
                while (bin_min < bin_mid)
                {
                    if (MatchBitapScore(d, loc + bin_mid, loc, pattern)
                        <= score_threshold)
                    {
                        bin_min = bin_mid;
                    }
                    else
                    {
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
                for (int j = finish; j >= start; j--)
                {
                    int charMatch;
                    if (text.Length <= j - 1 || !s.ContainsKey(text[j - 1]))
                    {
                        // Out of range.
                        charMatch = 0;
                    }
                    else
                    {
                        charMatch = s[text[j - 1]];
                    }
                    if (d == 0)
                    {
                        // First pass: exact match.
                        rd[j] = ((rd[j + 1] << 1) | 1) & charMatch;
                    }
                    else
                    {
                        // Subsequent passes: fuzzy match.
                        rd[j] = ((rd[j + 1] << 1) | 1) & charMatch
                                | (((last_rd[j + 1] | last_rd[j]) << 1) | 1) | last_rd[j + 1];
                    }
                    if ((rd[j] & matchmask) != 0)
                    {
                        double score = MatchBitapScore(d, j - 1, loc, pattern);
                        // This match will almost certainly be better than any existing
                        // match.  But check anyway.
                        if (score <= score_threshold)
                        {
                            // Told you so.
                            score_threshold = score;
                            best_loc = j - 1;
                            if (best_loc > loc)
                            {
                                // When passing loc, don't exceed our current distance from loc.
                                start = Math.Max(1, 2 * loc - best_loc);
                            }
                            else
                            {
                                // Already passed loc, downhill from here on in.
                                break;
                            }
                        }
                    }
                }
                if (MatchBitapScore(d + 1, loc, loc, pattern) > score_threshold)
                {
                    // No hope for a (better) match at greater error levels.
                    break;
                }
                last_rd = rd;
            }
            return best_loc;
        }

        /// <summary>
        /// Compute and return the score for a match with e errors and x location.
        /// </summary>
        /// <param name="e">Number of errors in match.</param>
        /// <param name="x">Location of match.</param>
        /// <param name="loc">Expected location of match.</param>
        /// <param name="pattern">Pattern being sought.</param>
        /// <returns>Overall score for match (0.0 = good, 1.0 = bad).</returns>
        private double MatchBitapScore(int e, int x, int loc, string pattern)
        {
            float accuracy = (float)e / pattern.Length;
            int proximity = Math.Abs(loc - x);
            if (Match_Distance == 0)
            {
                // Dodge divide by zero error.
                return proximity == 0 ? accuracy : 1.0;
            }
            return accuracy + (proximity / (float)Match_Distance);
        }

        /// <summary>
        /// Initialise the alphabet for the Bitap algorithm.
        /// </summary>
        /// <param name="pattern">The text to encode.</param>
        /// <returns>Hash of character locations.</returns>
        protected Dictionary<char, int> MatchAlphabet(string pattern)
        {
            Dictionary<char, int> s = new Dictionary<char, int>();
            char[] char_pattern = pattern.ToCharArray();
            foreach (char c in char_pattern)
            {
                if (!s.ContainsKey(c))
                {
                    s.Add(c, 0);
                }
            }
            int i = 0;
            foreach (char c in char_pattern)
            {
                int value = s[c] | (1 << (pattern.Length - i - 1));
                s[c] = value;
                i++;
            }
            return s;
        }
    }
}