using System;
using NUnit.Framework;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CollabEdit.VersionControl.Tests
{
    [TestFixture]
    public class TestRepositoryBranch
    {

        [Test]
        public void RepositoryBranch_Test_EmtyBranch_HasEmptyHistory()
        {
            var repository = new RepositoryBranch<string, string>();

            Assert.That(repository.Head == null);
            Assert.That(repository.GetHistory().Count() == 0, "Empty repository should have zero length history");
        }

        [Test]
        public void RepositoryBranch_Test_CommitOperation()
        {
            var repository = new RepositoryBranch<string, string>();
            Assert.That(repository.Head == null);

            var commit = repository.Commit(string.Empty, string.Empty);
            Assert.That(repository.Head.Equals(commit), "Repo should poit to the last commit");
            Assert.That(commit.Previous == null, "First commit should not have any parents");

            var commit2 = repository.Commit("other", string.Empty);
            Assert.That(repository.Head.Equals(commit2), "Repo should poit to the last commit");
            Assert.That(commit2.Previous != null, "New commit should point to previous one");
            Assert.That(commit2.Previous.Equals(commit), "New commit should point to previous one");
            Assert.That(repository.GetHistory().Count() == 2, "Repository should contains exactly two commits at this point");
            Assert.That(repository.GetHistory().First().Equals(commit2), "History should starts from last commit");
        }

        [Test]
        public void RepositoryBranch_Test_CommitTheSameValue()
        {
            var repository = new RepositoryBranch<string, string>();
            const string value = "some string";
            var valueChars = new char[value.Length];
            value.CopyTo(0, valueChars, 0, value.Length);
            string theCopyValue = new String(valueChars);

            var firstCommit = repository.Commit(value, string.Empty);
            Assert.That(firstCommit != null, "Commit may not return null value");

            var secondCommit = repository.Commit(theCopyValue, string.Empty);
            Assert.That(firstCommit.Equals(secondCommit), "Commit of the current value should not produce new version");
            Assert.That(firstCommit == secondCommit, "Commit of the current value should return the current commit instance");
        }

        [Test]
        public void RepositoryBranch_Test_Merge_EqualBranch()
        {

        }
    }

}