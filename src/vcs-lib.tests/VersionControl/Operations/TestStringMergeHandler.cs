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

            var stub = new MergeScriptStub(lDiff, rDiff);
            stub.Align();
            Assert.That(stub.ALeft.Count, Is.EqualTo(stub.ARight.Count));
        }

        private class MergeScriptStub : MergeScript
        {
            public MergeScriptStub(List<Diff> left, List<Diff> right) : base(left, right) { }

            public new List<Diff> ALeft => base.ALeft;
            public new List<Diff> ARight => base.ARight;
            public void Align()
            {
                base.AlignScripts();
            }
        }
    }
}