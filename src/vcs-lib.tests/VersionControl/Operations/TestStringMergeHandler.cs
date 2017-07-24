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
        [TestCase("Fox jumps", "Brown fox jumps", "Fox jumps over frog")]
        [TestCase("Fox jumps over frog", "Brown fox jumps over frog", "Fox jumps")]
        [TestCase("Fox jumps", "Brown fox jumps", "Blue fox jumps")]
        public void TestCompareDiffs(string origin, string left, string right)
        {
            var diffOps = new DiffOperations();
            var lDiff = diffOps.GetDifference(origin, left);
            var rDiff = diffOps.GetDifference(origin, right);

            lDiff.Align(rDiff);
            Assert.That(lDiff.Count, Is.EqualTo(rDiff.Count));
        }
    }
}