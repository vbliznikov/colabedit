using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Google.DiffMatchPatch;

namespace CollabEdit.VersionControl.Operations.Tests
{
    [TestFixture]
    public class TestStringMergeHandler
    {
        [TestCase("Fox jumps",
            "Brown fox jumps",
                  "Fox jumps over frog",
            "Brown fox jumps over frog")]
        [TestCase("Fox jumps over frog",
            "Brown fox jumps over frog",
                  "Fox jumps",
            "Brown fox jumps")]
        [TestCase("Fox jumps",
            "Brown fox jumps",
             "Blue fox jumps",
            "Brown fox jumps")]
        [TestCase("Fox eats the frog", "Pig grunt", "Pig grunt", "Pig grunt")]
        [TestCaseSource(typeof(StringMergeCases))]
        public void TestMergeWords(string origin, string left, string right, string expected)
        {
            var mergeHandler = new StringMergeHandler();
            var mergeResult = mergeHandler.Merge(origin, left, right, ConflictResolutionOptions.RaiseException);
            Assert.That(mergeResult, Is.EqualTo(expected));
        }
    }

    internal class StringMergeCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return LineInsertions();
        }

        public TestCaseData LineInsertions()
        {
            var args = new string[4];
            args[0] = "\n\r";                                      // origin
            args[1] = "New line above.\n\r\n\r";                   // left
            args[2] = "\n\rNew line below.\n\r";                   // right
            args[3] = "New line above.\n\r\n\rNew line below.\n\r";//expected
            return new TestCaseData(args).SetName("New Line insertions left and right");
        }
    }
}