using NUnit.Framework;

namespace CollabEdit.VersionControl.Operations.Tests
{
    [TestFixture]
    public class TestMergeUtils
    {

        [TestCase(1, 2, ConflictResolutionOptions.TakeLeft, ExpectedResult = 1)]
        [TestCase(1, 2, ConflictResolutionOptions.TakeRight, ExpectedResult = 2)]
        public int Test_ResolveConflicts(int left, int right, ConflictResolutionOptions options)
        {
            return MergeUtils.ResolveConflict(left, right, options);
        }

        [Test]
        public void Test_ResolveConflicts_WithException()
        {
            Assert.Throws(typeof(MergeOperationException),
                () => MergeUtils.ResolveConflict(1, 2, ConflictResolutionOptions.RaiseException), "Should throw exception");
        }

        [TestCase(1, 1, ConflictResolutionOptions.RaiseException, ExpectedResult = 1)]
        [TestCase(1, 2, ConflictResolutionOptions.TakeLeft, ExpectedResult = 1)]
        [TestCase(1, 2, ConflictResolutionOptions.TakeRight, ExpectedResult = 2)]
        public int Test_MergeTwoValues_WithOptions(int left, int right, ConflictResolutionOptions options)
        {
            return MergeUtils.Merge(left, right, options);
        }


        [TestCase("", "", ConflictResolutionOptions.RaiseException, ExpectedResult = "")]
        [TestCase("abc", "abc", ConflictResolutionOptions.RaiseException, ExpectedResult = "abc")]
        [TestCase("abc", "abC", ConflictResolutionOptions.TakeLeft, ExpectedResult = "abc")]
        [TestCase("abc", "abC", ConflictResolutionOptions.TakeRight, ExpectedResult = "abC")]
        [TestCase(null, null, ConflictResolutionOptions.RaiseException, ExpectedResult = null)]
        [TestCase("", null, ConflictResolutionOptions.TakeLeft, ExpectedResult = "")]
        [TestCase(null, "", ConflictResolutionOptions.TakeLeft, ExpectedResult = null)]
        [TestCase("", null, ConflictResolutionOptions.TakeRight, ExpectedResult = null)]
        [TestCase(null, "", ConflictResolutionOptions.TakeRight, ExpectedResult = "")]
        public string Test_MergeTwoRefValues_WithOptions(string left, string right, ConflictResolutionOptions options)
        {
            return MergeUtils.Merge(left, right, options);
        }

        [Test]
        public void Test_MergeTwoValues_WithException()
        {
            Assert.Throws(typeof(MergeOperationException), () => MergeUtils.Merge(1, 2, ConflictResolutionOptions.RaiseException),
                "Should throw MergeOperationException");
        }

        [Test]
        public void Test_MergeTwoRefValues_WithException()
        {
            Assert.Throws(typeof(MergeOperationException), () => MergeUtils.Merge(null, "", ConflictResolutionOptions.RaiseException),
                "Should throw MergeOperationException");
        }

        [Test]
        public void Test_MergeTwoValues_WithDefaultOption()
        {
            Assert.Throws(typeof(MergeOperationException), () => MergeUtils.Merge(1, 2), "Should throw MergeOperationException by default");
        }

        [TestCase(null, null, null, ConflictResolutionOptions.RaiseException, ExpectedResult = null)]
        [TestCase("null", "null", "null", ConflictResolutionOptions.RaiseException, ExpectedResult = "null")]
        [TestCase("", null, null, ConflictResolutionOptions.RaiseException, ExpectedResult = null)]
        [TestCase(null, "null", "null", ConflictResolutionOptions.RaiseException, ExpectedResult = "null")]
        [TestCase(null, null, "null", ConflictResolutionOptions.RaiseException, ExpectedResult = "null")]
        [TestCase(null, "null", null, ConflictResolutionOptions.RaiseException, ExpectedResult = "null")]
        [TestCase("null", "abc", "null", ConflictResolutionOptions.RaiseException, ExpectedResult = "abc")]
        [TestCase("null", "null", "abc", ConflictResolutionOptions.RaiseException, ExpectedResult = "abc")]
        [TestCase("null", "abcd", "abc", ConflictResolutionOptions.TakeRight, ExpectedResult = "abc")]
        [TestCase("null", "abc", "abcd", ConflictResolutionOptions.TakeLeft, ExpectedResult = "abc")]
        public string Test_TwoWayMerge_WithOptions(string origin, string left, string right, ConflictResolutionOptions options)
        {
            return MergeUtils.Merge(origin, left, right, options);
        }

        [Test]
        public void Test_TwoWayMerge_WithException()
        {
            Assert.Throws(typeof(MergeOperationException), () => MergeUtils.Merge("", "abc", "cde", ConflictResolutionOptions.RaiseException),
                "Should throw MergeOperationException");
        }

        [Test]
        public void Test_TwoWayMerge_WithDefaultOpion()
        {
            Assert.Throws(typeof(MergeOperationException), () => MergeUtils.Merge("", "abc", "cde"),
                "Should throw MergeOperationException");
        }
    }
}