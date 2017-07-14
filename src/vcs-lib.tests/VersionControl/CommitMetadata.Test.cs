using System;
using NUnit.Framework;

namespace CollabEdit.VersionControl.Tests
{
    [TestFixture]
    public class TestCommitMetadata
    {
        [TestCase("", "", ExpectedResult = true)]
        [TestCase("text", "text", ExpectedResult = true)]
        [TestCase("text", "", ExpectedResult = false)]
        [TestCase("text", "text1", ExpectedResult = false)]
        public bool Test_CommitMetadata_GetHashCode(string first, string second)
        {
            var firstMeta = new CommitMetadata(first);
            var secondMeta = new CommitMetadata(second);

            return firstMeta.GetHashCode() == secondMeta.GetHashCode();
        }

        [TestCase("", "", ExpectedResult = true)]
        [TestCase("text", "text", ExpectedResult = true)]
        [TestCase("text", "", ExpectedResult = false)]
        [TestCase("text", "text1", ExpectedResult = false)]
        public bool Test_CommitMetadata_Equals(string first, string second)
        {
            var firstMeta = new CommitMetadata(first);
            var secondMeta = new CommitMetadata(second);

            return firstMeta.Equals(secondMeta);
        }
    }

}