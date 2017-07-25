using System;
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
        public void TestCompareDiffs(string origin, string left, string right, string expected)
        {
            var mergeHandler = new StringMergeHandler();
            var mergeResult = mergeHandler.Merge(origin, left, right, ConflictResolutionOptions.RaiseException);
            Assert.That(mergeResult, Is.EqualTo(expected));
        }
    }
}