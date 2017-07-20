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
 * Diff Match and Patch -- Test Harness
 * http://code.google.com/p/google-diff-match-patch/
 */

/* Minor refactoring (code split) for better readability and to match C# naming style
*  Author: v.bliznikov@gmail.com
*/

using DiffMatchPatch;
using System.Collections.Generic;
using System;
using NUnit.Framework;

namespace nicTest
{
    [TestFixture()]
    public class TestPatchOperations : PatchOperations
    {

        [Test, Ignore("broken")]
        public void patch_patchObjTest()
        {
            // Patch Object.
            Patch p = new Patch();
            p.start1 = 20;
            p.start2 = 21;
            p.length1 = 18;
            p.length2 = 17;
            p.diffs = new List<Diff> {
          new Diff(Operation.EQUAL, "jump"),
          new Diff(Operation.DELETE, "s"),
          new Diff(Operation.INSERT, "ed"),
          new Diff(Operation.EQUAL, " over "),
          new Diff(Operation.DELETE, "the"),
          new Diff(Operation.INSERT, "a"),
          new Diff(Operation.EQUAL, "\nlaz")};
            string strp = "@@ -21,18 +22,17 @@\n jump\n-s\n+ed\n  over \n-the\n+a\n %0alaz\n";
            Assert.AreEqual(strp, p.ToString(), "Patch: toString.");
        }

        [Test, Ignore("broken")]
        public void patch_fromTextTest()
        {
            TestPatchOperations dmp = new TestPatchOperations();
            Assert.IsTrue(dmp.PatchFromText("").Count == 0, "patch_fromText: #0.");

            string strp = "@@ -21,18 +22,17 @@\n jump\n-s\n+ed\n  over \n-the\n+a\n %0alaz\n";
            Assert.AreEqual(strp, dmp.PatchFromText(strp)[0].ToString(), "patch_fromText: #1.");

            Assert.AreEqual("@@ -1 +1 @@\n-a\n+b\n", dmp.PatchFromText("@@ -1 +1 @@\n-a\n+b\n")[0].ToString(), "patch_fromText: #2.");

            Assert.AreEqual("@@ -1,3 +0,0 @@\n-abc\n", dmp.PatchFromText("@@ -1,3 +0,0 @@\n-abc\n")[0].ToString(), "patch_fromText: #3.");

            Assert.AreEqual("@@ -0,0 +1,3 @@\n+abc\n", dmp.PatchFromText("@@ -0,0 +1,3 @@\n+abc\n")[0].ToString(), "patch_fromText: #4.");

            // Generates error.
            try
            {
                dmp.PatchFromText("Bad\nPatch\n");
                Assert.Fail("patch_fromText: #5.");
            }
            catch (ArgumentException)
            {
                // Exception expected.
            }
        }

        [Test, Ignore("Broken")]
        public void patch_toTextTest()
        {
            TestPatchOperations dmp = new TestPatchOperations();
            string strp = "@@ -21,18 +22,17 @@\n jump\n-s\n+ed\n  over \n-the\n+a\n  laz\n";
            List<Patch> patches;
            patches = dmp.PatchFromText(strp);
            string result = dmp.PatchToText(patches);
            Assert.AreEqual(strp, result);

            strp = "@@ -1,9 +1,9 @@\n-f\n+F\n oo+fooba\n@@ -7,9 +7,9 @@\n obar\n-,\n+.\n  tes\n";
            patches = dmp.PatchFromText(strp);
            result = dmp.PatchToText(patches);
            Assert.AreEqual(strp, result);
        }

        [Test()]
        public void patch_addContextTest()
        {
            TestPatchOperations dmp = new TestPatchOperations();
            dmp.Patch_Margin = 4;
            Patch p;
            p = dmp.PatchFromText("@@ -21,4 +21,10 @@\n-jump\n+somersault\n")[0];
            dmp.PatchAaddContext(p, "The quick brown fox jumps over the lazy dog.");
            Assert.AreEqual("@@ -17,12 +17,18 @@\n fox \n-jump\n+somersault\n s ov\n", p.ToString(), "patch_addContext: Simple case.");

            p = dmp.PatchFromText("@@ -21,4 +21,10 @@\n-jump\n+somersault\n")[0];
            dmp.PatchAaddContext(p, "The quick brown fox jumps.");
            Assert.AreEqual("@@ -17,10 +17,16 @@\n fox \n-jump\n+somersault\n s.\n", p.ToString(), "patch_addContext: Not enough trailing context.");

            p = dmp.PatchFromText("@@ -3 +3,2 @@\n-e\n+at\n")[0];
            dmp.PatchAaddContext(p, "The quick brown fox jumps.");
            Assert.AreEqual("@@ -1,7 +1,8 @@\n Th\n-e\n+at\n  qui\n", p.ToString(), "patch_addContext: Not enough leading context.");

            p = dmp.PatchFromText("@@ -3 +3,2 @@\n-e\n+at\n")[0];
            dmp.PatchAaddContext(p, "The quick brown fox jumps.  The quick brown fox crashes.");
            Assert.AreEqual("@@ -1,27 +1,28 @@\n Th\n-e\n+at\n  quick brown fox jumps. \n", p.ToString(), "patch_addContext: Ambiguity.");
        }

        [Test, Ignore("broken")]
        public void patch_makeTest()
        {
            TestPatchOperations dmp = new TestPatchOperations();
            List<Patch> patches;
            patches = dmp.PatchMake("", "");
            Assert.AreEqual("", dmp.PatchToText(patches), "patch_make: Null case.");

            string text1 = "The quick brown fox jumps over the lazy dog.";
            string text2 = "That quick brown fox jumped over a lazy dog.";
            string expectedPatch = "@@ -1,8 +1,7 @@\n Th\n-at\n+e\n  qui\n@@ -21,17 +21,18 @@\n jump\n-ed\n+s\n  over \n-a\n+the\n  laz\n";
            // The second patch must be "-21,17 +21,18", not "-22,17 +21,18" due to rolling context.
            patches = dmp.PatchMake(text2, text1);
            Assert.AreEqual(expectedPatch, dmp.PatchToText(patches), "patch_make: Text2+Text1 inputs.");

            expectedPatch = "@@ -1,11 +1,12 @@\n Th\n-e\n+at\n  quick b\n@@ -22,18 +22,17 @@\n jump\n-s\n+ed\n  over \n-the\n+a\n  laz\n";
            patches = dmp.PatchMake(text1, text2);
            Assert.AreEqual(expectedPatch, dmp.PatchToText(patches), "patch_make: Text1+Text2 inputs.");

            List<Diff> diffs = dmp.DiffOps.GetDifference(text1, text2, false);
            patches = dmp.PatchMake(diffs);
            Assert.AreEqual(expectedPatch, dmp.PatchToText(patches), "patch_make: Diff input.");

            patches = dmp.PatchMake(text1, diffs);
            Assert.AreEqual(expectedPatch, dmp.PatchToText(patches), "patch_make: Text1+Diff inputs.");

            patches = dmp.PatchMake(text1, text2, diffs);
            Assert.AreEqual(expectedPatch, dmp.PatchToText(patches), "patch_make: Text1+Text2+Diff inputs (deprecated).");

            patches = dmp.PatchMake("`1234567890-=[]\\;',./", "~!@#$%^&*()_+{}|:\"<>?");
            Assert.AreEqual("@@ -1,21 +1,21 @@\n-%601234567890-=%5b%5d%5c;',./\n+~!@#$%25%5e&*()_+%7b%7d%7c:%22%3c%3e?\n",
                dmp.PatchToText(patches),
                "patch_toText: Character encoding.");

            diffs = new List<Diff> {
          new Diff(Operation.DELETE, "`1234567890-=[]\\;',./"),
          new Diff(Operation.INSERT, "~!@#$%^&*()_+{}|:\"<>?")};
            CollectionAssert.AreEqual(diffs,
                dmp.PatchFromText("@@ -1,21 +1,21 @@\n-%601234567890-=%5B%5D%5C;',./\n+~!@#$%25%5E&*()_+%7B%7D%7C:%22%3C%3E?\n")[0].diffs,
                "patch_fromText: Character decoding.");

            text1 = "";
            for (int x = 0; x < 100; x++)
            {
                text1 += "abcdef";
            }
            text2 = text1 + "123";
            expectedPatch = "@@ -573,28 +573,31 @@\n cdefabcdefabcdefabcdefabcdef\n+123\n";
            patches = dmp.PatchMake(text1, text2);
            Assert.AreEqual(expectedPatch, dmp.PatchToText(patches), "patch_make: Long string with repeats.");

            // Test null inputs -- not needed because nulls can't be passed in C#.
        }

        [Test, Ignore("broken")]
        public void patch_splitMaxTest()
        {
            // Assumes that Match_MaxBits is 32.
            TestPatchOperations dmp = new TestPatchOperations();
            List<Patch> patches;

            patches = dmp.PatchMake("abcdefghijklmnopqrstuvwxyz01234567890", "XabXcdXefXghXijXklXmnXopXqrXstXuvXwxXyzX01X23X45X67X89X0");
            dmp.PatchSplitMax(patches);
            Assert.AreEqual("@@ -1,32 +1,46 @@\n+X\n ab\n+X\n cd\n+X\n ef\n+X\n gh\n+X\n ij\n+X\n kl\n+X\n mn\n+X\n op\n+X\n qr\n+X\n st\n+X\n uv\n+X\n wx\n+X\n yz\n+X\n 012345\n@@ -25,13 +39,18 @@\n zX01\n+X\n 23\n+X\n 45\n+X\n 67\n+X\n 89\n+X\n 0\n", dmp.PatchToText(patches));

            patches = dmp.PatchMake("abcdef1234567890123456789012345678901234567890123456789012345678901234567890uvwxyz", "abcdefuvwxyz");
            string oldToText = dmp.PatchToText(patches);
            dmp.PatchSplitMax(patches);
            Assert.AreEqual(oldToText, dmp.PatchToText(patches));

            patches = dmp.PatchMake("1234567890123456789012345678901234567890123456789012345678901234567890", "abc");
            dmp.PatchSplitMax(patches);
            Assert.AreEqual("@@ -1,32 +1,4 @@\n-1234567890123456789012345678\n 9012\n@@ -29,32 +1,4 @@\n-9012345678901234567890123456\n 7890\n@@ -57,14 +1,3 @@\n-78901234567890\n+abc\n", dmp.PatchToText(patches));

            patches = dmp.PatchMake("abcdefghij , h : 0 , t : 1 abcdefghij , h : 0 , t : 1 abcdefghij , h : 0 , t : 1", "abcdefghij , h : 1 , t : 1 abcdefghij , h : 1 , t : 1 abcdefghij , h : 0 , t : 1");
            dmp.PatchSplitMax(patches);
            Assert.AreEqual("@@ -2,32 +2,32 @@\n bcdefghij , h : \n-0\n+1\n  , t : 1 abcdef\n@@ -29,32 +29,32 @@\n bcdefghij , h : \n-0\n+1\n  , t : 1 abcdef\n", dmp.PatchToText(patches));
        }

        [Test()]
        public void patch_addPaddingTest()
        {
            TestPatchOperations dmp = new TestPatchOperations();
            List<Patch> patches;
            patches = dmp.PatchMake("", "test");
            Assert.AreEqual("@@ -0,0 +1,4 @@\n+test\n",
               dmp.PatchToText(patches),
               "patch_addPadding: Both edges full.");
            dmp.PatchAddPadding(patches);
            Assert.AreEqual("@@ -1,8 +1,12 @@\n %01%02%03%04\n+test\n %01%02%03%04\n",
                dmp.PatchToText(patches),
                "patch_addPadding: Both edges full.");

            patches = dmp.PatchMake("XY", "XtestY");
            Assert.AreEqual("@@ -1,2 +1,6 @@\n X\n+test\n Y\n",
                dmp.PatchToText(patches),
                "patch_addPadding: Both edges partial.");
            dmp.PatchAddPadding(patches);
            Assert.AreEqual("@@ -2,8 +2,12 @@\n %02%03%04X\n+test\n Y%01%02%03\n",
                dmp.PatchToText(patches),
                "patch_addPadding: Both edges partial.");

            patches = dmp.PatchMake("XXXXYYYY", "XXXXtestYYYY");
            Assert.AreEqual("@@ -1,8 +1,12 @@\n XXXX\n+test\n YYYY\n",
                dmp.PatchToText(patches),
                "patch_addPadding: Both edges none.");
            dmp.PatchAddPadding(patches);
            Assert.AreEqual("@@ -5,8 +5,12 @@\n XXXX\n+test\n YYYY\n",
               dmp.PatchToText(patches),
               "patch_addPadding: Both edges none.");
        }

        [Test()]
        public void patch_applyTest()
        {
            TestPatchOperations dmp = new TestPatchOperations();
            dmp.Match.Match_Distance = 1000;
            dmp.Match.Match_Threshold = 0.5f;
            dmp.Patch_DeleteThreshold = 0.5f;
            List<Patch> patches;
            patches = dmp.PatchMake("", "");
            Object[] results = dmp.PatchApply(patches, "Hello world.");
            bool[] boolArray = (bool[])results[1];
            string resultStr = results[0] + "\t" + boolArray.Length;
            Assert.AreEqual("Hello world.\t0", resultStr, "patch_apply: Null case.");

            patches = dmp.PatchMake("The quick brown fox jumps over the lazy dog.", "That quick brown fox jumped over a lazy dog.");
            results = dmp.PatchApply(patches, "The quick brown fox jumps over the lazy dog.");
            boolArray = (bool[])results[1];
            resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
            Assert.AreEqual("That quick brown fox jumped over a lazy dog.\tTrue\tTrue", resultStr, "patch_apply: Exact match.");

            results = dmp.PatchApply(patches, "The quick red rabbit jumps over the tired tiger.");
            boolArray = (bool[])results[1];
            resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
            Assert.AreEqual("That quick red rabbit jumped over a tired tiger.\tTrue\tTrue", resultStr, "patch_apply: Partial match.");

            results = dmp.PatchApply(patches, "I am the very model of a modern major general.");
            boolArray = (bool[])results[1];
            resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
            Assert.AreEqual("I am the very model of a modern major general.\tFalse\tFalse", resultStr, "patch_apply: Failed match.");

            patches = dmp.PatchMake("x1234567890123456789012345678901234567890123456789012345678901234567890y", "xabcy");
            results = dmp.PatchApply(patches, "x123456789012345678901234567890-----++++++++++-----123456789012345678901234567890y");
            boolArray = (bool[])results[1];
            resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
            Assert.AreEqual("xabcy\tTrue\tTrue", resultStr, "patch_apply: Big delete, small change.");

            patches = dmp.PatchMake("x1234567890123456789012345678901234567890123456789012345678901234567890y", "xabcy");
            results = dmp.PatchApply(patches, "x12345678901234567890---------------++++++++++---------------12345678901234567890y");
            boolArray = (bool[])results[1];
            resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
            Assert.AreEqual("xabc12345678901234567890---------------++++++++++---------------12345678901234567890y\tFalse\tTrue", resultStr, "patch_apply: Big delete, big change 1.");

            dmp.Patch_DeleteThreshold = 0.6f;
            patches = dmp.PatchMake("x1234567890123456789012345678901234567890123456789012345678901234567890y", "xabcy");
            results = dmp.PatchApply(patches, "x12345678901234567890---------------++++++++++---------------12345678901234567890y");
            boolArray = (bool[])results[1];
            resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
            Assert.AreEqual("xabcy\tTrue\tTrue", resultStr, "patch_apply: Big delete, big change 2.");
            dmp.Patch_DeleteThreshold = 0.5f;

            dmp.Match.Match_Threshold = 0.0f;
            dmp.Match.Match_Distance = 0;
            patches = dmp.PatchMake("abcdefghijklmnopqrstuvwxyz--------------------1234567890", "abcXXXXXXXXXXdefghijklmnopqrstuvwxyz--------------------1234567YYYYYYYYYY890");
            results = dmp.PatchApply(patches, "ABCDEFGHIJKLMNOPQRSTUVWXYZ--------------------1234567890");
            boolArray = (bool[])results[1];
            resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
            Assert.AreEqual("ABCDEFGHIJKLMNOPQRSTUVWXYZ--------------------1234567YYYYYYYYYY890\tFalse\tTrue", resultStr, "patch_apply: Compensate for failed patch.");
            dmp.Match.Match_Threshold = 0.5f;
            dmp.Match.Match_Distance = 1000;

            patches = dmp.PatchMake("", "test");
            string patchStr = dmp.PatchToText(patches);
            dmp.PatchApply(patches, "");
            Assert.AreEqual(patchStr, dmp.PatchToText(patches), "patch_apply: No side effects.");

            patches = dmp.PatchMake("The quick brown fox jumps over the lazy dog.", "Woof");
            patchStr = dmp.PatchToText(patches);
            dmp.PatchApply(patches, "The quick brown fox jumps over the lazy dog.");
            Assert.AreEqual(patchStr, dmp.PatchToText(patches), "patch_apply: No side effects with major delete.");

            patches = dmp.PatchMake("", "test");
            results = dmp.PatchApply(patches, "");
            boolArray = (bool[])results[1];
            resultStr = results[0] + "\t" + boolArray[0];
            Assert.AreEqual("test\tTrue", resultStr, "patch_apply: Edge exact match.");

            patches = dmp.PatchMake("XY", "XtestY");
            results = dmp.PatchApply(patches, "XY");
            boolArray = (bool[])results[1];
            resultStr = results[0] + "\t" + boolArray[0];
            Assert.AreEqual("XtestY\tTrue", resultStr, "patch_apply: Near edge exact match.");

            patches = dmp.PatchMake("y", "y123");
            results = dmp.PatchApply(patches, "x");
            boolArray = (bool[])results[1];
            resultStr = results[0] + "\t" + boolArray[0];
            Assert.AreEqual("x123\tTrue", resultStr, "patch_apply: Edge partial match.");
        }
    }
}
