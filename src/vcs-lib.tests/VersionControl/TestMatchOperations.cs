using System.Collections.Generic;
using DiffMatchPatch;
using NUnit.Framework;

namespace nicTest
{
    [TestFixture]
    public class TestMatchOperations : MatchOperations
    {
        [Test]
        public void match_alphabetTest() {
            var dmp = new TestMatchOperations();
            // Initialise the bitmasks for Bitap.
            var bitmask = new Dictionary<char, int> {{'a', 4}, {'b', 2}, {'c', 1}};
            CollectionAssert.AreEqual(bitmask, dmp.match_alphabet("abc"), "match_alphabet: Unique.");

            bitmask.Clear();
            bitmask.Add('a', 37); bitmask.Add('b', 18); bitmask.Add('c', 8);
            CollectionAssert.AreEqual(bitmask, dmp.match_alphabet("abcaba"), "match_alphabet: Duplicates.");
        }

        [Test]
        public void match_bitapTest() {
            var dmp = new TestMatchOperations
            {
                Match_Distance = 100,
                Match_Threshold = 0.5f
            };

            // Bitap algorithm.
            Assert.AreEqual(5, dmp.match_bitap("abcdefghijk", "fgh", 5), "match_bitap: Exact match #1.");

            Assert.AreEqual(5, dmp.match_bitap("abcdefghijk", "fgh", 0), "match_bitap: Exact match #2.");

            Assert.AreEqual(4, dmp.match_bitap("abcdefghijk", "efxhi", 0), "match_bitap: Fuzzy match #1.");

            Assert.AreEqual(2, dmp.match_bitap("abcdefghijk", "cdefxyhijk", 5), "match_bitap: Fuzzy match #2.");

            Assert.AreEqual(-1, dmp.match_bitap("abcdefghijk", "bxy", 1), "match_bitap: Fuzzy match #3.");

            Assert.AreEqual(2, dmp.match_bitap("123456789xx0", "3456789x0", 2), "match_bitap: Overflow.");

            Assert.AreEqual(0, dmp.match_bitap("abcdef", "xxabc", 4), "match_bitap: Before start match.");

            Assert.AreEqual(3, dmp.match_bitap("abcdef", "defyy", 4), "match_bitap: Beyond end match.");

            Assert.AreEqual(0, dmp.match_bitap("abcdef", "xabcdefy", 0), "match_bitap: Oversized pattern.");

            dmp.Match_Threshold = 0.4f;
            Assert.AreEqual(4, dmp.match_bitap("abcdefghijk", "efxyhi", 1), "match_bitap: Threshold #1.");

            dmp.Match_Threshold = 0.3f;
            Assert.AreEqual(-1, dmp.match_bitap("abcdefghijk", "efxyhi", 1), "match_bitap: Threshold #2.");

            dmp.Match_Threshold = 0.0f;
            Assert.AreEqual(1, dmp.match_bitap("abcdefghijk", "bcdef", 1), "match_bitap: Threshold #3.");

            dmp.Match_Threshold = 0.5f;
            Assert.AreEqual(0, dmp.match_bitap("abcdexyzabcde", "abccde", 3), "match_bitap: Multiple select #1.");

            Assert.AreEqual(8, dmp.match_bitap("abcdexyzabcde", "abccde", 5), "match_bitap: Multiple select #2.");

            dmp.Match_Distance = 10;  // Strict location.
            Assert.AreEqual(-1, dmp.match_bitap("abcdefghijklmnopqrstuvwxyz", "abcdefg", 24), "match_bitap: Distance test #1.");

            Assert.AreEqual(0, dmp.match_bitap("abcdefghijklmnopqrstuvwxyz", "abcdxxefg", 1), "match_bitap: Distance test #2.");

            dmp.Match_Distance = 1000;  // Loose location.
            Assert.AreEqual(0, dmp.match_bitap("abcdefghijklmnopqrstuvwxyz", "abcdefg", 24), "match_bitap: Distance test #3.");
        }

        [Test]
        public void match_mainTest() {
            var dmp = new TestMatchOperations();
            // Full match.
            Assert.AreEqual(0, dmp.match_main("abcdef", "abcdef", 1000), "match_main: Equality.");

            Assert.AreEqual(-1, dmp.match_main("", "abcdef", 1), "match_main: Null text.");

            Assert.AreEqual(3, dmp.match_main("abcdef", "", 3), "match_main: Null pattern.");

            Assert.AreEqual(3, dmp.match_main("abcdef", "de", 3), "match_main: Exact match.");

            Assert.AreEqual(3, dmp.match_main("abcdef", "defy", 4), "match_main: Beyond end match.");

            Assert.AreEqual(0, dmp.match_main("abcdef", "abcdefy", 0), "match_main: Oversized pattern.");

            dmp.Match_Threshold = 0.7f;
            Assert.AreEqual(4, dmp.match_main("I am the very model of a modern major general.", " that berry ", 5), "match_main: Complex match.");
            dmp.Match_Threshold = 0.5f;

            // Test null inputs -- not needed because nulls can't be passed in C#.
        }
    }
}