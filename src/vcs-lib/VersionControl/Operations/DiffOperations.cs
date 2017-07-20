using System;

namespace DiffMatchPatch
{
    public class DiffOperations : MatchOperations
    {
        /// <summary>
        /// Determine the common prefix of two strings.
        /// </summary>
        /// <param name="text1">First string.</param>
        /// <param name="text2">Second string.</param>
        /// <returns>The number of characters common to the start of each string.</returns>
        public int diff_commonPrefix(string text1, string text2) {
            // Performance analysis: http://neil.fraser.name/news/2007/10/09/
            int n = Math.Min(text1.Length, text2.Length);
            for (int i = 0; i < n; i++) {
                if (text1[i] != text2[i]) {
                    return i;
                }
            }
            return n;
        }

        /// <summary>
        /// Determine the common suffix of two strings.
        /// </summary>
        /// <param name="text1">First string.</param>
        /// <param name="text2">Second string.</param>
        /// <returns>The number of characters common to the end of each string.</returns>
        public int diff_commonSuffix(string text1, string text2) {
            // Performance analysis: http://neil.fraser.name/news/2007/10/09/
            int text1_length = text1.Length;
            int text2_length = text2.Length;
            int n = Math.Min(text1.Length, text2.Length);
            for (int i = 1; i <= n; i++) {
                if (text1[text1_length - i] != text2[text2_length - i]) {
                    return i - 1;
                }
            }
            return n;
        }
    }
}