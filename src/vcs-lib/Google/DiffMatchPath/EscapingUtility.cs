/*
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

namespace DiffMatchPatch
{
    public static class EscapingUtility
    {
        /// <summary>
        ///  Unescape selected chars for compatability with JavaScript's encodeURI.
        /// In speed critical applications this could be dropped since the
        /// receiving application will certainly decode these fine.
        /// </summary>
        /// <remarks>
        /// Note that this function is case-sensitive.  Thus "%3F" would not be
        /// unescaped.  But this is ok because it is only called with the output of
        /// Web​Utility.UrlEncode which returns lowercase hex.
        /// </remarks>
        /// <example>"%3f" -> "?", "%24" -> "$", etc.</example>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string UnescapeForEncodeUsiCcompatability(string str)
        {
            return str.Replace("%21", "!").Replace("%7e", "~")
                .Replace("%27", "'").Replace("%28", "(").Replace("%29", ")")
                .Replace("%3b", ";").Replace("%2f", "/").Replace("%3f", "?")
                .Replace("%3a", ":").Replace("%40", "@").Replace("%26", "&")
                .Replace("%3d", "=").Replace("%2b", "+").Replace("%24", "$")
                .Replace("%2c", ",").Replace("%23", "#");
        }
    }
}