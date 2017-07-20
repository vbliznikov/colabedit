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

using DiffMatchPatch;
using System.Collections.Generic;
using System;
using NUnit.Framework;

namespace nicTest {
  [TestFixture()]
  public class TestPatchOperations : PatchOperations {
      
    [Test, Ignore("broken")]
    public void patch_patchObjTest() {
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
    public void patch_fromTextTest() {
      TestPatchOperations dmp = new TestPatchOperations();
      Assert.IsTrue(dmp.patch_fromText("").Count == 0, "patch_fromText: #0.");

      string strp = "@@ -21,18 +22,17 @@\n jump\n-s\n+ed\n  over \n-the\n+a\n %0alaz\n";
      Assert.AreEqual(strp, dmp.patch_fromText(strp)[0].ToString(), "patch_fromText: #1.");

      Assert.AreEqual("@@ -1 +1 @@\n-a\n+b\n", dmp.patch_fromText("@@ -1 +1 @@\n-a\n+b\n")[0].ToString(), "patch_fromText: #2.");

      Assert.AreEqual("@@ -1,3 +0,0 @@\n-abc\n", dmp.patch_fromText("@@ -1,3 +0,0 @@\n-abc\n") [0].ToString(), "patch_fromText: #3.");

      Assert.AreEqual("@@ -0,0 +1,3 @@\n+abc\n", dmp.patch_fromText("@@ -0,0 +1,3 @@\n+abc\n") [0].ToString(), "patch_fromText: #4.");

      // Generates error.
      try {
        dmp.patch_fromText("Bad\nPatch\n");
        Assert.Fail("patch_fromText: #5.");
      } catch (ArgumentException) {
        // Exception expected.
      }
    }

    [Test, Ignore("Broken")]
    public void patch_toTextTest() {
      TestPatchOperations dmp = new TestPatchOperations();
      string strp = "@@ -21,18 +22,17 @@\n jump\n-s\n+ed\n  over \n-the\n+a\n  laz\n";
      List<Patch> patches;
      patches = dmp.patch_fromText(strp);
      string result = dmp.patch_toText(patches);
      Assert.AreEqual(strp, result);

      strp = "@@ -1,9 +1,9 @@\n-f\n+F\n oo+fooba\n@@ -7,9 +7,9 @@\n obar\n-,\n+.\n  tes\n";
      patches = dmp.patch_fromText(strp);
      result = dmp.patch_toText(patches);
      Assert.AreEqual(strp, result);
    }

    [Test()]
    public void patch_addContextTest() {
      TestPatchOperations dmp = new TestPatchOperations();
      dmp.Patch_Margin = 4;
      Patch p;
      p = dmp.patch_fromText("@@ -21,4 +21,10 @@\n-jump\n+somersault\n") [0];
      dmp.patch_addContext(p, "The quick brown fox jumps over the lazy dog.");
      Assert.AreEqual("@@ -17,12 +17,18 @@\n fox \n-jump\n+somersault\n s ov\n", p.ToString(), "patch_addContext: Simple case.");

      p = dmp.patch_fromText("@@ -21,4 +21,10 @@\n-jump\n+somersault\n")[0];
      dmp.patch_addContext(p, "The quick brown fox jumps.");
      Assert.AreEqual("@@ -17,10 +17,16 @@\n fox \n-jump\n+somersault\n s.\n", p.ToString(), "patch_addContext: Not enough trailing context.");

      p = dmp.patch_fromText("@@ -3 +3,2 @@\n-e\n+at\n")[0];
      dmp.patch_addContext(p, "The quick brown fox jumps.");
      Assert.AreEqual("@@ -1,7 +1,8 @@\n Th\n-e\n+at\n  qui\n", p.ToString(), "patch_addContext: Not enough leading context.");

      p = dmp.patch_fromText("@@ -3 +3,2 @@\n-e\n+at\n")[0];
      dmp.patch_addContext(p, "The quick brown fox jumps.  The quick brown fox crashes.");
      Assert.AreEqual("@@ -1,27 +1,28 @@\n Th\n-e\n+at\n  quick brown fox jumps. \n", p.ToString(), "patch_addContext: Ambiguity.");
    }

    [Test, Ignore("broken")]
    public void patch_makeTest() {
      TestPatchOperations dmp = new TestPatchOperations();
      List<Patch> patches;
      patches = dmp.patch_make("", "");
      Assert.AreEqual("", dmp.patch_toText(patches), "patch_make: Null case.");

      string text1 = "The quick brown fox jumps over the lazy dog.";
      string text2 = "That quick brown fox jumped over a lazy dog.";
      string expectedPatch = "@@ -1,8 +1,7 @@\n Th\n-at\n+e\n  qui\n@@ -21,17 +21,18 @@\n jump\n-ed\n+s\n  over \n-a\n+the\n  laz\n";
      // The second patch must be "-21,17 +21,18", not "-22,17 +21,18" due to rolling context.
      patches = dmp.patch_make(text2, text1);
      Assert.AreEqual(expectedPatch, dmp.patch_toText(patches), "patch_make: Text2+Text1 inputs.");

      expectedPatch = "@@ -1,11 +1,12 @@\n Th\n-e\n+at\n  quick b\n@@ -22,18 +22,17 @@\n jump\n-s\n+ed\n  over \n-the\n+a\n  laz\n";
      patches = dmp.patch_make(text1, text2);
      Assert.AreEqual(expectedPatch, dmp.patch_toText(patches), "patch_make: Text1+Text2 inputs.");

      List<Diff> diffs = dmp.DiffOps.diff_main(text1, text2, false);
      patches = dmp.patch_make(diffs);
      Assert.AreEqual(expectedPatch, dmp.patch_toText(patches), "patch_make: Diff input.");

      patches = dmp.patch_make(text1, diffs);
      Assert.AreEqual(expectedPatch, dmp.patch_toText(patches), "patch_make: Text1+Diff inputs.");

      patches = dmp.patch_make(text1, text2, diffs);
      Assert.AreEqual(expectedPatch, dmp.patch_toText(patches), "patch_make: Text1+Text2+Diff inputs (deprecated).");

      patches = dmp.patch_make("`1234567890-=[]\\;',./", "~!@#$%^&*()_+{}|:\"<>?");
      Assert.AreEqual("@@ -1,21 +1,21 @@\n-%601234567890-=%5b%5d%5c;',./\n+~!@#$%25%5e&*()_+%7b%7d%7c:%22%3c%3e?\n",
          dmp.patch_toText(patches),
          "patch_toText: Character encoding.");

      diffs = new List<Diff> {
          new Diff(Operation.DELETE, "`1234567890-=[]\\;',./"),
          new Diff(Operation.INSERT, "~!@#$%^&*()_+{}|:\"<>?")};
      CollectionAssert.AreEqual(diffs,
          dmp.patch_fromText("@@ -1,21 +1,21 @@\n-%601234567890-=%5B%5D%5C;',./\n+~!@#$%25%5E&*()_+%7B%7D%7C:%22%3C%3E?\n") [0].diffs,
          "patch_fromText: Character decoding.");

      text1 = "";
      for (int x = 0; x < 100; x++) {
        text1 += "abcdef";
      }
      text2 = text1 + "123";
      expectedPatch = "@@ -573,28 +573,31 @@\n cdefabcdefabcdefabcdefabcdef\n+123\n";
      patches = dmp.patch_make(text1, text2);
      Assert.AreEqual(expectedPatch, dmp.patch_toText(patches), "patch_make: Long string with repeats.");

      // Test null inputs -- not needed because nulls can't be passed in C#.
    }

    [Test, Ignore("broken")]
    public void patch_splitMaxTest() {
      // Assumes that Match_MaxBits is 32.
      TestPatchOperations dmp = new TestPatchOperations();
      List<Patch> patches;

      patches = dmp.patch_make("abcdefghijklmnopqrstuvwxyz01234567890", "XabXcdXefXghXijXklXmnXopXqrXstXuvXwxXyzX01X23X45X67X89X0");
      dmp.patch_splitMax(patches);
      Assert.AreEqual("@@ -1,32 +1,46 @@\n+X\n ab\n+X\n cd\n+X\n ef\n+X\n gh\n+X\n ij\n+X\n kl\n+X\n mn\n+X\n op\n+X\n qr\n+X\n st\n+X\n uv\n+X\n wx\n+X\n yz\n+X\n 012345\n@@ -25,13 +39,18 @@\n zX01\n+X\n 23\n+X\n 45\n+X\n 67\n+X\n 89\n+X\n 0\n", dmp.patch_toText(patches));

      patches = dmp.patch_make("abcdef1234567890123456789012345678901234567890123456789012345678901234567890uvwxyz", "abcdefuvwxyz");
      string oldToText = dmp.patch_toText(patches);
      dmp.patch_splitMax(patches);
      Assert.AreEqual(oldToText, dmp.patch_toText(patches));

      patches = dmp.patch_make("1234567890123456789012345678901234567890123456789012345678901234567890", "abc");
      dmp.patch_splitMax(patches);
      Assert.AreEqual("@@ -1,32 +1,4 @@\n-1234567890123456789012345678\n 9012\n@@ -29,32 +1,4 @@\n-9012345678901234567890123456\n 7890\n@@ -57,14 +1,3 @@\n-78901234567890\n+abc\n", dmp.patch_toText(patches));

      patches = dmp.patch_make("abcdefghij , h : 0 , t : 1 abcdefghij , h : 0 , t : 1 abcdefghij , h : 0 , t : 1", "abcdefghij , h : 1 , t : 1 abcdefghij , h : 1 , t : 1 abcdefghij , h : 0 , t : 1");
      dmp.patch_splitMax(patches);
      Assert.AreEqual("@@ -2,32 +2,32 @@\n bcdefghij , h : \n-0\n+1\n  , t : 1 abcdef\n@@ -29,32 +29,32 @@\n bcdefghij , h : \n-0\n+1\n  , t : 1 abcdef\n", dmp.patch_toText(patches));
    }

    [Test()]
    public void patch_addPaddingTest() {
      TestPatchOperations dmp = new TestPatchOperations();
      List<Patch> patches;
      patches = dmp.patch_make("", "test");
      Assert.AreEqual("@@ -0,0 +1,4 @@\n+test\n",
         dmp.patch_toText(patches),
         "patch_addPadding: Both edges full.");
      dmp.patch_addPadding(patches);
      Assert.AreEqual("@@ -1,8 +1,12 @@\n %01%02%03%04\n+test\n %01%02%03%04\n",
          dmp.patch_toText(patches),
          "patch_addPadding: Both edges full.");

      patches = dmp.patch_make("XY", "XtestY");
      Assert.AreEqual("@@ -1,2 +1,6 @@\n X\n+test\n Y\n",
          dmp.patch_toText(patches),
          "patch_addPadding: Both edges partial.");
      dmp.patch_addPadding(patches);
      Assert.AreEqual("@@ -2,8 +2,12 @@\n %02%03%04X\n+test\n Y%01%02%03\n",
          dmp.patch_toText(patches),
          "patch_addPadding: Both edges partial.");

      patches = dmp.patch_make("XXXXYYYY", "XXXXtestYYYY");
      Assert.AreEqual("@@ -1,8 +1,12 @@\n XXXX\n+test\n YYYY\n",
          dmp.patch_toText(patches),
          "patch_addPadding: Both edges none.");
      dmp.patch_addPadding(patches);
      Assert.AreEqual("@@ -5,8 +5,12 @@\n XXXX\n+test\n YYYY\n",
         dmp.patch_toText(patches),
         "patch_addPadding: Both edges none.");
    }

    [Test()]
    public void patch_applyTest() {
      TestPatchOperations dmp = new TestPatchOperations();
      dmp.Match.Match_Distance = 1000;
      dmp.Match.Match_Threshold = 0.5f;
      dmp.Patch_DeleteThreshold = 0.5f;
      List<Patch> patches;
      patches = dmp.patch_make("", "");
      Object[] results = dmp.patch_apply(patches, "Hello world.");
      bool[] boolArray = (bool[])results[1];
      string resultStr = results[0] + "\t" + boolArray.Length;
      Assert.AreEqual("Hello world.\t0", resultStr, "patch_apply: Null case.");

      patches = dmp.patch_make("The quick brown fox jumps over the lazy dog.", "That quick brown fox jumped over a lazy dog.");
      results = dmp.patch_apply(patches, "The quick brown fox jumps over the lazy dog.");
      boolArray = (bool[])results[1];
      resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
      Assert.AreEqual("That quick brown fox jumped over a lazy dog.\tTrue\tTrue", resultStr, "patch_apply: Exact match.");

      results = dmp.patch_apply(patches, "The quick red rabbit jumps over the tired tiger.");
      boolArray = (bool[])results[1];
      resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
      Assert.AreEqual("That quick red rabbit jumped over a tired tiger.\tTrue\tTrue", resultStr, "patch_apply: Partial match.");

      results = dmp.patch_apply(patches, "I am the very model of a modern major general.");
      boolArray = (bool[])results[1];
      resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
      Assert.AreEqual("I am the very model of a modern major general.\tFalse\tFalse", resultStr, "patch_apply: Failed match.");

      patches = dmp.patch_make("x1234567890123456789012345678901234567890123456789012345678901234567890y", "xabcy");
      results = dmp.patch_apply(patches, "x123456789012345678901234567890-----++++++++++-----123456789012345678901234567890y");
      boolArray = (bool[])results[1];
      resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
      Assert.AreEqual("xabcy\tTrue\tTrue", resultStr, "patch_apply: Big delete, small change.");

      patches = dmp.patch_make("x1234567890123456789012345678901234567890123456789012345678901234567890y", "xabcy");
      results = dmp.patch_apply(patches, "x12345678901234567890---------------++++++++++---------------12345678901234567890y");
      boolArray = (bool[])results[1];
      resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
      Assert.AreEqual("xabc12345678901234567890---------------++++++++++---------------12345678901234567890y\tFalse\tTrue", resultStr, "patch_apply: Big delete, big change 1.");

      dmp.Patch_DeleteThreshold = 0.6f;
      patches = dmp.patch_make("x1234567890123456789012345678901234567890123456789012345678901234567890y", "xabcy");
      results = dmp.patch_apply(patches, "x12345678901234567890---------------++++++++++---------------12345678901234567890y");
      boolArray = (bool[])results[1];
      resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
      Assert.AreEqual("xabcy\tTrue\tTrue", resultStr, "patch_apply: Big delete, big change 2.");
      dmp.Patch_DeleteThreshold = 0.5f;

      dmp.Match.Match_Threshold = 0.0f;
      dmp.Match.Match_Distance = 0;
      patches = dmp.patch_make("abcdefghijklmnopqrstuvwxyz--------------------1234567890", "abcXXXXXXXXXXdefghijklmnopqrstuvwxyz--------------------1234567YYYYYYYYYY890");
      results = dmp.patch_apply(patches, "ABCDEFGHIJKLMNOPQRSTUVWXYZ--------------------1234567890");
      boolArray = (bool[])results[1];
      resultStr = results[0] + "\t" + boolArray[0] + "\t" + boolArray[1];
      Assert.AreEqual("ABCDEFGHIJKLMNOPQRSTUVWXYZ--------------------1234567YYYYYYYYYY890\tFalse\tTrue", resultStr, "patch_apply: Compensate for failed patch.");
      dmp.Match.Match_Threshold = 0.5f;
      dmp.Match.Match_Distance = 1000;

      patches = dmp.patch_make("", "test");
      string patchStr = dmp.patch_toText(patches);
      dmp.patch_apply(patches, "");
      Assert.AreEqual(patchStr, dmp.patch_toText(patches), "patch_apply: No side effects.");

      patches = dmp.patch_make("The quick brown fox jumps over the lazy dog.", "Woof");
      patchStr = dmp.patch_toText(patches);
      dmp.patch_apply(patches, "The quick brown fox jumps over the lazy dog.");
      Assert.AreEqual(patchStr, dmp.patch_toText(patches), "patch_apply: No side effects with major delete.");

      patches = dmp.patch_make("", "test");
      results = dmp.patch_apply(patches, "");
      boolArray = (bool[])results[1];
      resultStr = results[0] + "\t" + boolArray[0];
      Assert.AreEqual("test\tTrue", resultStr, "patch_apply: Edge exact match.");

      patches = dmp.patch_make("XY", "XtestY");
      results = dmp.patch_apply(patches, "XY");
      boolArray = (bool[])results[1];
      resultStr = results[0] + "\t" + boolArray[0];
      Assert.AreEqual("XtestY\tTrue", resultStr, "patch_apply: Near edge exact match.");

      patches = dmp.patch_make("y", "y123");
      results = dmp.patch_apply(patches, "x");
      boolArray = (bool[])results[1];
      resultStr = results[0] + "\t" + boolArray[0];
      Assert.AreEqual("x123\tTrue", resultStr, "patch_apply: Edge partial match.");
    }
  }
}
