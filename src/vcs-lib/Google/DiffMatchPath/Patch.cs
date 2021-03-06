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
using System.Text;

namespace Google.DiffMatchPatch
{
    ///<summary>
    /// Class representing one patch operation.
    /// </summary> 
    public class Patch
    {
        public List<Diff> diffs = new List<Diff>();
        public int start1;
        public int start2;
        public int length1;
        public int length2;

        /**
     * Emmulate GNU diff's format.
     * Header: @@ -382,8 +481,9 @@
     * Indicies are printed as 1-based, not 0-based.
     * @return The GNU diff string.
     */
        public override string ToString()
        {
            string coords1, coords2;
            if (this.length1 == 0)
            {
                coords1 = this.start1 + ",0";
            }
            else if (this.length1 == 1)
            {
                coords1 = Convert.ToString(this.start1 + 1);
            }
            else
            {
                coords1 = (this.start1 + 1) + "," + this.length1;
            }
            if (this.length2 == 0)
            {
                coords2 = this.start2 + ",0";
            }
            else if (this.length2 == 1)
            {
                coords2 = Convert.ToString(this.start2 + 1);
            }
            else
            {
                coords2 = (this.start2 + 1) + "," + this.length2;
            }
            StringBuilder text = new StringBuilder();
            text.Append("@@ -").Append(coords1).Append(" +").Append(coords2)
                .Append(" @@\n");
            // Escape the body of the patch with %xx notation.
            foreach (Diff aDiff in this.diffs)
            {
                switch (aDiff.operation)
                {
                    case Operation.INSERT:
                        text.Append('+');
                        break;
                    case Operation.DELETE:
                        text.Append('-');
                        break;
                    case Operation.EQUAL:
                        text.Append(' ');
                        break;
                }

                text.Append(System.Net.WebUtility.UrlEncode(aDiff.text).Replace('+', ' ')).Append("\n");
            }

            return EscapingUtility.UnescapeForEncodeUsiCcompatability(
                text.ToString());
        }
    }
}