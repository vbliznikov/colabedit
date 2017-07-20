using System;
using System.Collections.Generic;
using System.Text;
using DiffMatchPatch;
using NUnit.Framework;

namespace nicTest
{
    public class TestDiffOperations : DiffOperations
    {
        [Test()]
        public void diff_commonPrefixTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Detect any common suffix.
            // Null case.
            Assert.AreEqual(0, dmp.diff_commonPrefix("abc", "xyz"));

            // Non-null case.
            Assert.AreEqual(4, dmp.diff_commonPrefix("1234abcdef", "1234xyz"));

            // Whole case.
            Assert.AreEqual(4, dmp.diff_commonPrefix("1234", "1234xyz"));
        }

        [Test()]
        public void diff_commonSuffixTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Detect any common suffix.
            // Null case.
            Assert.AreEqual(0, dmp.diff_commonSuffix("abc", "xyz"));

            // Non-null case.
            Assert.AreEqual(4, dmp.diff_commonSuffix("abcdef1234", "xyz1234"));

            // Whole case.
            Assert.AreEqual(4, dmp.diff_commonSuffix("1234", "xyz1234"));
        }

        [Test()]
        public void diff_commonOverlapTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Detect any suffix/prefix overlap.
            // Null case.
            Assert.AreEqual(0, dmp.diff_commonOverlap("", "abcd"));

            // Whole case.
            Assert.AreEqual(3, dmp.diff_commonOverlap("abc", "abcd"));

            // No overlap.
            Assert.AreEqual(0, dmp.diff_commonOverlap("123456", "abcd"));

            // Overlap.
            Assert.AreEqual(3, dmp.diff_commonOverlap("123456xxx", "xxxabcd"));

            // Unicode.
            // Some overly clever languages (C#) may treat ligatures as equal to their
            // component letters.  E.g. U+FB01 == 'fi'
            Assert.AreEqual(0, dmp.diff_commonOverlap("fi", "\ufb01i"));
        }

        [Test()]
        public void diff_halfmatchTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            dmp.Diff_Timeout = 1;
            // No match.
            Assert.IsNull(dmp.diff_halfMatch("1234567890", "abcdef"));

            Assert.IsNull(dmp.diff_halfMatch("12345", "23"));

            // Single Match.
            CollectionAssert.AreEqual(new string[] { "12", "90", "a", "z", "345678" }, dmp.diff_halfMatch("1234567890", "a345678z"));

            CollectionAssert.AreEqual(new string[] { "a", "z", "12", "90", "345678" }, dmp.diff_halfMatch("a345678z", "1234567890"));

            CollectionAssert.AreEqual(new string[] { "abc", "z", "1234", "0", "56789" }, dmp.diff_halfMatch("abc56789z", "1234567890"));

            CollectionAssert.AreEqual(new string[] { "a", "xyz", "1", "7890", "23456" }, dmp.diff_halfMatch("a23456xyz", "1234567890"));

            // Multiple Matches.
            CollectionAssert.AreEqual(new string[] { "12123", "123121", "a", "z", "1234123451234" }, dmp.diff_halfMatch("121231234123451234123121", "a1234123451234z"));

            CollectionAssert.AreEqual(new string[] { "", "-=-=-=-=-=", "x", "", "x-=-=-=-=-=-=-=" }, dmp.diff_halfMatch("x-=-=-=-=-=-=-=-=-=-=-=-=", "xx-=-=-=-=-=-=-="));

            CollectionAssert.AreEqual(new string[] { "-=-=-=-=-=", "", "", "y", "-=-=-=-=-=-=-=y" }, dmp.diff_halfMatch("-=-=-=-=-=-=-=-=-=-=-=-=y", "-=-=-=-=-=-=-=yy"));

            // Non-optimal halfmatch.
            // Optimal diff would be -q+x=H-i+e=lloHe+Hu=llo-Hew+y not -qHillo+x=HelloHe-w+Hulloy
            CollectionAssert.AreEqual(new string[] { "qHillo", "w", "x", "Hulloy", "HelloHe" }, dmp.diff_halfMatch("qHilloHelloHew", "xHelloHeHulloy"));

            // Optimal no halfmatch.
            dmp.Diff_Timeout = 0;
            Assert.IsNull(dmp.diff_halfMatch("qHilloHelloHew", "xHelloHeHulloy"));
        }

        [Test()]
        public void diff_linesToCharsTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Convert lines down to characters.
            List<string> tmpVector = new List<string>();
            tmpVector.Add("");
            tmpVector.Add("alpha\n");
            tmpVector.Add("beta\n");
            Object[] result = dmp.diff_linesToChars("alpha\nbeta\nalpha\n", "beta\nalpha\nbeta\n");
            Assert.AreEqual("\u0001\u0002\u0001", result[0]);
            Assert.AreEqual("\u0002\u0001\u0002", result[1]);
            CollectionAssert.AreEqual(tmpVector, (List<string>)result[2]);

            tmpVector.Clear();
            tmpVector.Add("");
            tmpVector.Add("alpha\r\n");
            tmpVector.Add("beta\r\n");
            tmpVector.Add("\r\n");
            result = dmp.diff_linesToChars("", "alpha\r\nbeta\r\n\r\n\r\n");
            Assert.AreEqual("", result[0]);
            Assert.AreEqual("\u0001\u0002\u0003\u0003", result[1]);
            CollectionAssert.AreEqual(tmpVector, (List<string>)result[2]);

            tmpVector.Clear();
            tmpVector.Add("");
            tmpVector.Add("a");
            tmpVector.Add("b");
            result = dmp.diff_linesToChars("a", "b");
            Assert.AreEqual("\u0001", result[0]);
            Assert.AreEqual("\u0002", result[1]);
            CollectionAssert.AreEqual(tmpVector, (List<string>)result[2]);

            // More than 256 to reveal any 8-bit limitations.
            int n = 300;
            tmpVector.Clear();
            StringBuilder lineList = new StringBuilder();
            StringBuilder charList = new StringBuilder();
            for (int x = 1; x < n + 1; x++) {
                tmpVector.Add(x + "\n");
                lineList.Append(x + "\n");
                charList.Append(Convert.ToChar(x));
            }
            Assert.AreEqual(n, tmpVector.Count);
            string lines = lineList.ToString();
            string chars = charList.ToString();
            Assert.AreEqual(n, chars.Length);
            tmpVector.Insert(0, "");
            result = dmp.diff_linesToChars(lines, "");
            Assert.AreEqual(chars, result[0]);
            Assert.AreEqual("", result[1]);
            CollectionAssert.AreEqual(tmpVector, (List<string>)result[2]);
        }

        [Test()]
        public void diff_charsToLinesTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Convert chars up to lines.
            List<Diff> diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "\u0001\u0002\u0001"),
                new Diff(Operation.INSERT, "\u0002\u0001\u0002")};
            List<string> tmpVector = new List<string>();
            tmpVector.Add("");
            tmpVector.Add("alpha\n");
            tmpVector.Add("beta\n");
            dmp.diff_charsToLines(diffs, tmpVector);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.EQUAL, "alpha\nbeta\nalpha\n"),
                new Diff(Operation.INSERT, "beta\nalpha\nbeta\n")}, diffs);

            // More than 256 to reveal any 8-bit limitations.
            int n = 300;
            tmpVector.Clear();
            StringBuilder lineList = new StringBuilder();
            StringBuilder charList = new StringBuilder();
            for (int x = 1; x < n + 1; x++) {
                tmpVector.Add(x + "\n");
                lineList.Append(x + "\n");
                charList.Append(Convert.ToChar (x));
            }
            Assert.AreEqual(n, tmpVector.Count);
            string lines = lineList.ToString();
            string chars = charList.ToString();
            Assert.AreEqual(n, chars.Length);
            tmpVector.Insert(0, "");
            diffs = new List<Diff> {new Diff(Operation.DELETE, chars)};
            dmp.diff_charsToLines(diffs, tmpVector);
            CollectionAssert.AreEqual(new List<Diff>
                {new Diff(Operation.DELETE, lines)}, diffs);
        }

        [Test()]
        public void diff_cleanupMergeTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Cleanup a messy diff.
            // Null case.
            List<Diff> diffs = new List<Diff>();
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff>(), diffs);

            // No change case.
            diffs = new List<Diff> {new Diff(Operation.EQUAL, "a"), new Diff(Operation.DELETE, "b"), new Diff(Operation.INSERT, "c")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.EQUAL, "a"), new Diff(Operation.DELETE, "b"), new Diff(Operation.INSERT, "c")}, diffs);

            // Merge equalities.
            diffs = new List<Diff> {new Diff(Operation.EQUAL, "a"), new Diff(Operation.EQUAL, "b"), new Diff(Operation.EQUAL, "c")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.EQUAL, "abc")}, diffs);

            // Merge deletions.
            diffs = new List<Diff> {new Diff(Operation.DELETE, "a"), new Diff(Operation.DELETE, "b"), new Diff(Operation.DELETE, "c")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.DELETE, "abc")}, diffs);

            // Merge insertions.
            diffs = new List<Diff> {new Diff(Operation.INSERT, "a"), new Diff(Operation.INSERT, "b"), new Diff(Operation.INSERT, "c")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.INSERT, "abc")}, diffs);

            // Merge interweave.
            diffs = new List<Diff> {new Diff(Operation.DELETE, "a"), new Diff(Operation.INSERT, "b"), new Diff(Operation.DELETE, "c"), new Diff(Operation.INSERT, "d"), new Diff(Operation.EQUAL, "e"), new Diff(Operation.EQUAL, "f")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.DELETE, "ac"), new Diff(Operation.INSERT, "bd"), new Diff(Operation.EQUAL, "ef")}, diffs);

            // Prefix and suffix detection.
            diffs = new List<Diff> {new Diff(Operation.DELETE, "a"), new Diff(Operation.INSERT, "abc"), new Diff(Operation.DELETE, "dc")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.EQUAL, "a"), new Diff(Operation.DELETE, "d"), new Diff(Operation.INSERT, "b"), new Diff(Operation.EQUAL, "c")}, diffs);

            // Prefix and suffix detection with equalities.
            diffs = new List<Diff> {new Diff(Operation.EQUAL, "x"), new Diff(Operation.DELETE, "a"), new Diff(Operation.INSERT, "abc"), new Diff(Operation.DELETE, "dc"), new Diff(Operation.EQUAL, "y")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.EQUAL, "xa"), new Diff(Operation.DELETE, "d"), new Diff(Operation.INSERT, "b"), new Diff(Operation.EQUAL, "cy")}, diffs);

            // Slide edit left.
            diffs = new List<Diff> {new Diff(Operation.EQUAL, "a"), new Diff(Operation.INSERT, "ba"), new Diff(Operation.EQUAL, "c")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.INSERT, "ab"), new Diff(Operation.EQUAL, "ac")}, diffs);

            // Slide edit right.
            diffs = new List<Diff> {new Diff(Operation.EQUAL, "c"), new Diff(Operation.INSERT, "ab"), new Diff(Operation.EQUAL, "a")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.EQUAL, "ca"), new Diff(Operation.INSERT, "ba")}, diffs);

            // Slide edit left recursive.
            diffs = new List<Diff> {new Diff(Operation.EQUAL, "a"), new Diff(Operation.DELETE, "b"), new Diff(Operation.EQUAL, "c"), new Diff(Operation.DELETE, "ac"), new Diff(Operation.EQUAL, "x")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.DELETE, "abc"), new Diff(Operation.EQUAL, "acx")}, diffs);

            // Slide edit right recursive.
            diffs = new List<Diff> {new Diff(Operation.EQUAL, "x"), new Diff(Operation.DELETE, "ca"), new Diff(Operation.EQUAL, "c"), new Diff(Operation.DELETE, "b"), new Diff(Operation.EQUAL, "a")};
            dmp.diff_cleanupMerge(diffs);
            CollectionAssert.AreEqual(new List<Diff> {new Diff(Operation.EQUAL, "xca"), new Diff(Operation.DELETE, "cba")}, diffs);
        }

        [Test()]
        public void diff_cleanupSemanticLosslessTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Slide diffs to match logical boundaries.
            // Null case.
            List<Diff> diffs = new List<Diff>();
            dmp.diff_cleanupSemanticLossless(diffs);
            CollectionAssert.AreEqual(new List<Diff>(), diffs);

            // Blank lines.
            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "AAA\r\n\r\nBBB"),
                new Diff(Operation.INSERT, "\r\nDDD\r\n\r\nBBB"),
                new Diff(Operation.EQUAL, "\r\nEEE")
            };
            dmp.diff_cleanupSemanticLossless(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.EQUAL, "AAA\r\n\r\n"),
                new Diff(Operation.INSERT, "BBB\r\nDDD\r\n\r\n"),
                new Diff(Operation.EQUAL, "BBB\r\nEEE")}, diffs);

            // Line boundaries.
            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "AAA\r\nBBB"),
                new Diff(Operation.INSERT, " DDD\r\nBBB"),
                new Diff(Operation.EQUAL, " EEE")};
            dmp.diff_cleanupSemanticLossless(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.EQUAL, "AAA\r\n"),
                new Diff(Operation.INSERT, "BBB DDD\r\n"),
                new Diff(Operation.EQUAL, "BBB EEE")}, diffs);

            // Word boundaries.
            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "The c"),
                new Diff(Operation.INSERT, "ow and the c"),
                new Diff(Operation.EQUAL, "at.")};
            dmp.diff_cleanupSemanticLossless(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.EQUAL, "The "),
                new Diff(Operation.INSERT, "cow and the "),
                new Diff(Operation.EQUAL, "cat.")}, diffs);

            // Alphanumeric boundaries.
            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "The-c"),
                new Diff(Operation.INSERT, "ow-and-the-c"),
                new Diff(Operation.EQUAL, "at.")};
            dmp.diff_cleanupSemanticLossless(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.EQUAL, "The-"),
                new Diff(Operation.INSERT, "cow-and-the-"),
                new Diff(Operation.EQUAL, "cat.")}, diffs);

            // Hitting the start.
            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "a"),
                new Diff(Operation.DELETE, "a"),
                new Diff(Operation.EQUAL, "ax")};
            dmp.diff_cleanupSemanticLossless(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "a"),
                new Diff(Operation.EQUAL, "aax")}, diffs);

            // Hitting the end.
            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "xa"),
                new Diff(Operation.DELETE, "a"),
                new Diff(Operation.EQUAL, "a")};
            dmp.diff_cleanupSemanticLossless(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.EQUAL, "xaa"),
                new Diff(Operation.DELETE, "a")}, diffs);

            // Sentence boundaries.
            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "The xxx. The "),
                new Diff(Operation.INSERT, "zzz. The "),
                new Diff(Operation.EQUAL, "yyy.")};
            dmp.diff_cleanupSemanticLossless(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.EQUAL, "The xxx."),
                new Diff(Operation.INSERT, " The zzz."),
                new Diff(Operation.EQUAL, " The yyy.")}, diffs);
        }

        [Test()]
        public void diff_cleanupSemanticTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Cleanup semantically trivial equalities.
            // Null case.
            List<Diff> diffs = new List<Diff>();
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff>(), diffs);

            // No elimination #1.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "ab"),
                new Diff(Operation.INSERT, "cd"),
                new Diff(Operation.EQUAL, "12"),
                new Diff(Operation.DELETE, "e")};
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "ab"),
                new Diff(Operation.INSERT, "cd"),
                new Diff(Operation.EQUAL, "12"),
                new Diff(Operation.DELETE, "e")}, diffs);

            // No elimination #2.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "abc"),
                new Diff(Operation.INSERT, "ABC"),
                new Diff(Operation.EQUAL, "1234"),
                new Diff(Operation.DELETE, "wxyz")};
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "abc"),
                new Diff(Operation.INSERT, "ABC"),
                new Diff(Operation.EQUAL, "1234"),
                new Diff(Operation.DELETE, "wxyz")}, diffs);

            // Simple elimination.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "a"),
                new Diff(Operation.EQUAL, "b"),
                new Diff(Operation.DELETE, "c")};
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "abc"),
                new Diff(Operation.INSERT, "b")}, diffs);

            // Backpass elimination.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "ab"),
                new Diff(Operation.EQUAL, "cd"),
                new Diff(Operation.DELETE, "e"),
                new Diff(Operation.EQUAL, "f"),
                new Diff(Operation.INSERT, "g")};
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "abcdef"),
                new Diff(Operation.INSERT, "cdfg")}, diffs);

            // Multiple eliminations.
            diffs = new List<Diff> {
                new Diff(Operation.INSERT, "1"),
                new Diff(Operation.EQUAL, "A"),
                new Diff(Operation.DELETE, "B"),
                new Diff(Operation.INSERT, "2"),
                new Diff(Operation.EQUAL, "_"),
                new Diff(Operation.INSERT, "1"),
                new Diff(Operation.EQUAL, "A"),
                new Diff(Operation.DELETE, "B"),
                new Diff(Operation.INSERT, "2")};
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "AB_AB"),
                new Diff(Operation.INSERT, "1A2_1A2")}, diffs);

            // Word boundaries.
            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "The c"),
                new Diff(Operation.DELETE, "ow and the c"),
                new Diff(Operation.EQUAL, "at.")};
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.EQUAL, "The "),
                new Diff(Operation.DELETE, "cow and the "),
                new Diff(Operation.EQUAL, "cat.")}, diffs);

            // No overlap elimination.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "abcxx"),
                new Diff(Operation.INSERT, "xxdef")};
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "abcxx"),
                new Diff(Operation.INSERT, "xxdef")}, diffs);

            // Overlap elimination.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "abcxxx"),
                new Diff(Operation.INSERT, "xxxdef")};
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "abc"),
                new Diff(Operation.EQUAL, "xxx"),
                new Diff(Operation.INSERT, "def")}, diffs);

            // Reverse overlap elimination.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "xxxabc"),
                new Diff(Operation.INSERT, "defxxx")};
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.INSERT, "def"),
                new Diff(Operation.EQUAL, "xxx"),
                new Diff(Operation.DELETE, "abc")}, diffs);

            // Two overlap eliminations.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "abcd1212"),
                new Diff(Operation.INSERT, "1212efghi"),
                new Diff(Operation.EQUAL, "----"),
                new Diff(Operation.DELETE, "A3"),
                new Diff(Operation.INSERT, "3BC")};
            dmp.diff_cleanupSemantic(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "abcd"),
                new Diff(Operation.EQUAL, "1212"),
                new Diff(Operation.INSERT, "efghi"),
                new Diff(Operation.EQUAL, "----"),
                new Diff(Operation.DELETE, "A"),
                new Diff(Operation.EQUAL, "3"),
                new Diff(Operation.INSERT, "BC")}, diffs);
        }

        [Test()]
        public void diff_cleanupEfficiencyTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Cleanup operationally trivial equalities.
            dmp.Diff_EditCost = 4;
            // Null case.
            List<Diff> diffs = new List<Diff> ();
            dmp.diff_cleanupEfficiency(diffs);
            CollectionAssert.AreEqual(new List<Diff>(), diffs);

            // No elimination.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "ab"),
                new Diff(Operation.INSERT, "12"),
                new Diff(Operation.EQUAL, "wxyz"),
                new Diff(Operation.DELETE, "cd"),
                new Diff(Operation.INSERT, "34")};
            dmp.diff_cleanupEfficiency(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "ab"),
                new Diff(Operation.INSERT, "12"),
                new Diff(Operation.EQUAL, "wxyz"),
                new Diff(Operation.DELETE, "cd"),
                new Diff(Operation.INSERT, "34")}, diffs);

            // Four-edit elimination.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "ab"),
                new Diff(Operation.INSERT, "12"),
                new Diff(Operation.EQUAL, "xyz"),
                new Diff(Operation.DELETE, "cd"),
                new Diff(Operation.INSERT, "34")};
            dmp.diff_cleanupEfficiency(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "abxyzcd"),
                new Diff(Operation.INSERT, "12xyz34")}, diffs);

            // Three-edit elimination.
            diffs = new List<Diff> {
                new Diff(Operation.INSERT, "12"),
                new Diff(Operation.EQUAL, "x"),
                new Diff(Operation.DELETE, "cd"),
                new Diff(Operation.INSERT, "34")};
            dmp.diff_cleanupEfficiency(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "xcd"),
                new Diff(Operation.INSERT, "12x34")}, diffs);

            // Backpass elimination.
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "ab"),
                new Diff(Operation.INSERT, "12"),
                new Diff(Operation.EQUAL, "xy"),
                new Diff(Operation.INSERT, "34"),
                new Diff(Operation.EQUAL, "z"),
                new Diff(Operation.DELETE, "cd"),
                new Diff(Operation.INSERT, "56")};
            dmp.diff_cleanupEfficiency(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "abxyzcd"),
                new Diff(Operation.INSERT, "12xy34z56")}, diffs);

            // High cost elimination.
            dmp.Diff_EditCost = 5;
            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "ab"),
                new Diff(Operation.INSERT, "12"),
                new Diff(Operation.EQUAL, "wxyz"),
                new Diff(Operation.DELETE, "cd"),
                new Diff(Operation.INSERT, "34")};
            dmp.diff_cleanupEfficiency(diffs);
            CollectionAssert.AreEqual(new List<Diff> {
                new Diff(Operation.DELETE, "abwxyzcd"),
                new Diff(Operation.INSERT, "12wxyz34")}, diffs);
            dmp.Diff_EditCost = 4;
        }

        [Test()]
        public void diff_prettyHtmlTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Pretty print.
            List<Diff> diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "a\n"),
                new Diff(Operation.DELETE, "<B>b</B>"),
                new Diff(Operation.INSERT, "c&d")};
            Assert.AreEqual("<span>a&para;<br></span><del style=\"background:#ffe6e6;\">&lt;B&gt;b&lt;/B&gt;</del><ins style=\"background:#e6ffe6;\">c&amp;d</ins>",
                dmp.diff_prettyHtml(diffs));
        }

        [Test()]
        public void diff_textTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Compute the source and destination texts.
            List<Diff> diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "jump"),
                new Diff(Operation.DELETE, "s"),
                new Diff(Operation.INSERT, "ed"),
                new Diff(Operation.EQUAL, " over "),
                new Diff(Operation.DELETE, "the"),
                new Diff(Operation.INSERT, "a"),
                new Diff(Operation.EQUAL, " lazy")};
            Assert.AreEqual("jumps over the lazy", dmp.diff_text1(diffs));

            Assert.AreEqual("jumped over a lazy", dmp.diff_text2(diffs));
        }

        [Test, Ignore("broken")]
        public void diff_deltaTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Convert a diff into delta string.
            List<Diff> diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "jump"),
                new Diff(Operation.DELETE, "s"),
                new Diff(Operation.INSERT, "ed"),
                new Diff(Operation.EQUAL, " over "),
                new Diff(Operation.DELETE, "the"),
                new Diff(Operation.INSERT, "a"),
                new Diff(Operation.EQUAL, " lazy"),
                new Diff(Operation.INSERT, "old dog")};
            string text1 = dmp.diff_text1(diffs);
            Assert.AreEqual("jumps over the lazy", text1);

            string delta = dmp.diff_toDelta(diffs);
            Assert.AreEqual("=4\t-1\t+ed\t=6\t-3\t+a\t=5\t+old dog", delta);

            // Convert delta string into a diff.
            CollectionAssert.AreEqual(diffs, dmp.diff_fromDelta(text1, delta));

            // Generates error (19 < 20).
            try {
                dmp.diff_fromDelta(text1 + "x", delta);
                Assert.Fail("diff_fromDelta: Too long.");
            } catch (ArgumentException) {
                // Exception expected.
            }

            // Generates error (19 > 18).
            try {
                dmp.diff_fromDelta(text1.Substring(1), delta);
                Assert.Fail("diff_fromDelta: Too short.");
            } catch (ArgumentException) {
                // Exception expected.
            }

            // Generates error (%c3%xy invalid Unicode).
            try {
                dmp.diff_fromDelta("", "+%c3%xy");
                Assert.Fail("diff_fromDelta: Invalid character.");
            } catch (ArgumentException) {
                // Exception expected.
            }

            // Test deltas with special characters.
            char zero = (char)0;
            char one = (char)1;
            char two = (char)2;
            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "\u0680 " + zero + " \t %"),
                new Diff(Operation.DELETE, "\u0681 " + one + " \n ^"),
                new Diff(Operation.INSERT, "\u0682 " + two + " \\ |")};
            text1 = dmp.diff_text1(diffs);
            Assert.AreEqual("\u0680 " + zero + " \t %\u0681 " + one + " \n ^", text1);

            delta = dmp.diff_toDelta(diffs);
            // Lowercase, due to UrlEncode uses lower.
            Assert.AreEqual("=7\t-7\t+%da%82 %02 %5c %7c", delta, "diff_toDelta: Unicode.");

            CollectionAssert.AreEqual(diffs, dmp.diff_fromDelta(text1, delta), "diff_fromDelta: Unicode.");

            // Verify pool of unchanged characters.
            diffs = new List<Diff> {
                new Diff(Operation.INSERT, "A-Z a-z 0-9 - _ . ! ~ * ' ( ) ; / ? : @ & = + $ , # ")};
            string text2 = dmp.diff_text2(diffs);
            Assert.AreEqual("A-Z a-z 0-9 - _ . ! ~ * \' ( ) ; / ? : @ & = + $ , # ", text2, "diff_text2: Unchanged characters.");

            delta = dmp.diff_toDelta(diffs);
            Assert.AreEqual("+A-Z a-z 0-9 - _ . ! ~ * \' ( ) ; / ? : @ & = + $ , # ", delta, "diff_toDelta: Unchanged characters.");

            // Convert delta string into a diff.
            CollectionAssert.AreEqual(diffs, dmp.diff_fromDelta("", delta), "diff_fromDelta: Unchanged characters.");
        }

        [Test()]
        public void diff_xIndexTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Translate a location in text1 to text2.
            List<Diff> diffs = new List<Diff> {
                new Diff(Operation.DELETE, "a"),
                new Diff(Operation.INSERT, "1234"),
                new Diff(Operation.EQUAL, "xyz")};
            Assert.AreEqual(5, dmp.diff_xIndex(diffs, 2), "diff_xIndex: Translation on equality.");

            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "a"),
                new Diff(Operation.DELETE, "1234"),
                new Diff(Operation.EQUAL, "xyz")};
            Assert.AreEqual(1, dmp.diff_xIndex(diffs, 3), "diff_xIndex: Translation on deletion.");
        }

        [Test()]
        public void diff_levenshteinTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            List<Diff> diffs = new List<Diff> {
                new Diff(Operation.DELETE, "abc"),
                new Diff(Operation.INSERT, "1234"),
                new Diff(Operation.EQUAL, "xyz")};
            Assert.AreEqual(4, dmp.diff_levenshtein(diffs), "diff_levenshtein: Levenshtein with trailing equality.");

            diffs = new List<Diff> {
                new Diff(Operation.EQUAL, "xyz"),
                new Diff(Operation.DELETE, "abc"),
                new Diff(Operation.INSERT, "1234")};
            Assert.AreEqual(4, dmp.diff_levenshtein(diffs), "diff_levenshtein: Levenshtein with leading equality.");

            diffs = new List<Diff> {
                new Diff(Operation.DELETE, "abc"),
                new Diff(Operation.EQUAL, "xyz"),
                new Diff(Operation.INSERT, "1234")};
            Assert.AreEqual(7, dmp.diff_levenshtein(diffs), "diff_levenshtein: Levenshtein with middle equality.");
        }

        [Test()]
        public void diff_bisectTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Normal.
            string a = "cat";
            string b = "map";
            // Since the resulting diff hasn't been normalized, it would be ok if
            // the insertion and deletion pairs are swapped.
            // If the order changes, tweak this test as required.
            List<Diff> diffs = new List<Diff> {new Diff(Operation.DELETE, "c"), new Diff(Operation.INSERT, "m"), new Diff(Operation.EQUAL, "a"), new Diff(Operation.DELETE, "t"), new Diff(Operation.INSERT, "p")};
            CollectionAssert.AreEqual(diffs, dmp.diff_bisect(a, b, DateTime.MaxValue));

            // Timeout.
            diffs = new List<Diff> {new Diff(Operation.DELETE, "cat"), new Diff(Operation.INSERT, "map")};
            CollectionAssert.AreEqual(diffs, dmp.diff_bisect(a, b, DateTime.MinValue));
        }

        [Test()]
        public void diff_mainTest() {
            TestDiffOperations dmp = new TestDiffOperations();
            // Perform a trivial diff.
            List<Diff> diffs = new List<Diff> {};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("", "", false), "diff_main: Null case.");

            diffs = new List<Diff> {new Diff(Operation.EQUAL, "abc")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("abc", "abc", false), "diff_main: Equality.");

            diffs = new List<Diff> {new Diff(Operation.EQUAL, "ab"), new Diff(Operation.INSERT, "123"), new Diff(Operation.EQUAL, "c")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("abc", "ab123c", false), "diff_main: Simple insertion.");

            diffs = new List<Diff> {new Diff(Operation.EQUAL, "a"), new Diff(Operation.DELETE, "123"), new Diff(Operation.EQUAL, "bc")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("a123bc", "abc", false), "diff_main: Simple deletion.");

            diffs = new List<Diff> {new Diff(Operation.EQUAL, "a"), new Diff(Operation.INSERT, "123"), new Diff(Operation.EQUAL, "b"), new Diff(Operation.INSERT, "456"), new Diff(Operation.EQUAL, "c")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("abc", "a123b456c", false), "diff_main: Two insertions.");

            diffs = new List<Diff> {new Diff(Operation.EQUAL, "a"), new Diff(Operation.DELETE, "123"), new Diff(Operation.EQUAL, "b"), new Diff(Operation.DELETE, "456"), new Diff(Operation.EQUAL, "c")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("a123b456c", "abc", false), "diff_main: Two deletions.");

            // Perform a real diff.
            // Switch off the timeout.
            dmp.Diff_Timeout = 0;
            diffs = new List<Diff> {new Diff(Operation.DELETE, "a"), new Diff(Operation.INSERT, "b")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("a", "b", false), "diff_main: Simple case #1.");

            diffs = new List<Diff> {new Diff(Operation.DELETE, "Apple"), new Diff(Operation.INSERT, "Banana"), new Diff(Operation.EQUAL, "s are a"), new Diff(Operation.INSERT, "lso"), new Diff(Operation.EQUAL, " fruit.")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("Apples are a fruit.", "Bananas are also fruit.", false), "diff_main: Simple case #2.");

            diffs = new List<Diff> {new Diff(Operation.DELETE, "a"), new Diff(Operation.INSERT, "\u0680"), new Diff(Operation.EQUAL, "x"), new Diff(Operation.DELETE, "\t"), new Diff(Operation.INSERT, new string (new char[]{(char)0}))};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("ax\t", "\u0680x" + (char)0, false), "diff_main: Simple case #3.");

            diffs = new List<Diff> {new Diff(Operation.DELETE, "1"), new Diff(Operation.EQUAL, "a"), new Diff(Operation.DELETE, "y"), new Diff(Operation.EQUAL, "b"), new Diff(Operation.DELETE, "2"), new Diff(Operation.INSERT, "xab")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("1ayb2", "abxab", false), "diff_main: Overlap #1.");

            diffs = new List<Diff> {new Diff(Operation.INSERT, "xaxcx"), new Diff(Operation.EQUAL, "abc"), new Diff(Operation.DELETE, "y")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("abcy", "xaxcxabc", false), "diff_main: Overlap #2.");

            diffs = new List<Diff> {new Diff(Operation.DELETE, "ABCD"), new Diff(Operation.EQUAL, "a"), new Diff(Operation.DELETE, "="), new Diff(Operation.INSERT, "-"), new Diff(Operation.EQUAL, "bcd"), new Diff(Operation.DELETE, "="), new Diff(Operation.INSERT, "-"), new Diff(Operation.EQUAL, "efghijklmnopqrs"), new Diff(Operation.DELETE, "EFGHIJKLMNOefg")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("ABCDa=bcd=efghijklmnopqrsEFGHIJKLMNOefg", "a-bcd-efghijklmnopqrs", false), "diff_main: Overlap #3.");

            diffs = new List<Diff> {new Diff(Operation.INSERT, " "), new Diff(Operation.EQUAL, "a"), new Diff(Operation.INSERT, "nd"), new Diff(Operation.EQUAL, " [[Pennsylvania]]"), new Diff(Operation.DELETE, " and [[New")};
            CollectionAssert.AreEqual(diffs, dmp.diff_main("a [[Pennsylvania]] and [[New", " and [[Pennsylvania]]", false), "diff_main: Large equality.");

            dmp.Diff_Timeout = 0.1f;  // 100ms
            string a = "`Twas brillig, and the slithy toves\nDid gyre and gimble in the wabe:\nAll mimsy were the borogoves,\nAnd the mome raths outgrabe.\n";
            string b = "I am the very model of a modern major general,\nI've information vegetable, animal, and mineral,\nI know the kings of England, and I quote the fights historical,\nFrom Marathon to Waterloo, in order categorical.\n";
            // Increase the text lengths by 1024 times to ensure a timeout.
            for (int x = 0; x < 10; x++) {
                a = a + a;
                b = b + b;
            }
            DateTime startTime = DateTime.Now;
            dmp.diff_main(a, b);
            DateTime endTime = DateTime.Now;
            // Test that we took at least the timeout period.
            Assert.IsTrue(new TimeSpan(((long)(dmp.Diff_Timeout * 1000)) * 10000) <= endTime - startTime);
            // Test that we didn't take forever (be forgiving).
            // Theoretically this test could fail very occasionally if the
            // OS task swaps or locks up for a second at the wrong moment.
            Assert.IsTrue(new TimeSpan(((long)(dmp.Diff_Timeout * 1000)) * 10000 * 2) > endTime - startTime);
            dmp.Diff_Timeout = 0;

            // Test the linemode speedup.
            // Must be long to pass the 100 char cutoff.
            a = "1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n";
            b = "abcdefghij\nabcdefghij\nabcdefghij\nabcdefghij\nabcdefghij\nabcdefghij\nabcdefghij\nabcdefghij\nabcdefghij\nabcdefghij\nabcdefghij\nabcdefghij\nabcdefghij\n";
            CollectionAssert.AreEqual(dmp.diff_main(a, b, true), dmp.diff_main(a, b, false), "diff_main: Simple line-mode.");

            a = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
            b = "abcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghij";
            CollectionAssert.AreEqual(dmp.diff_main(a, b, true), dmp.diff_main(a, b, false), "diff_main: Single line-mode.");

            a = "1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n1234567890\n";
            b = "abcdefghij\n1234567890\n1234567890\n1234567890\nabcdefghij\n1234567890\n1234567890\n1234567890\nabcdefghij\n1234567890\n1234567890\n1234567890\nabcdefghij\n";
            string[] texts_linemode = diff_rebuildtexts(dmp.diff_main(a, b, true));
            string[] texts_textmode = diff_rebuildtexts(dmp.diff_main(a, b, false));
            CollectionAssert.AreEqual(texts_textmode, texts_linemode, "diff_main: Overlap line-mode.");

            // Test null inputs -- not needed because nulls can't be passed in C#.
        }

        private static string[] diff_rebuildtexts(List<Diff> diffs) {
            string[] text = { "", "" };
            foreach (Diff myDiff in diffs) {
                if (myDiff.operation != Operation.INSERT) {
                    text[0] += myDiff.text;
                }
                if (myDiff.operation != Operation.DELETE) {
                    text[1] += myDiff.text;
                }
            }
            return text;
        }
    }
}