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
            var repository = new Repository<int, string>();
            var commit = repository.CurrentBranch.Commit(0, "initial value");
            var secondBranch = repository.CreateBranch("second");
            Assert.That(secondBranch.Head.Equals(commit), "After creation Branch should point to the parent Head");

            var merge = secondBranch.MergeWith(repository.CurrentBranch);
            Assert.That(merge != null);
            Assert.That(merge.Equals(commit), "Merge op should return head before merge");
        }

        [Test]
        public void RepositoryBranch_Test_Merge_SourceBehindTarget()
        {
            var repository = new Repository<int, string>();
            var initialCommit = repository.CurrentBranch.Commit(0, "initial value");
            var secondBranch = repository.CreateBranch("second");
            Assert.That(secondBranch.Head.Equals(initialCommit), "After creation Branch should point to the parent Head");

            secondBranch.Commit(1, "second commit");
            var branchHead = secondBranch.Head;

            var merge = secondBranch.MergeWith(repository.CurrentBranch);
            Assert.That(merge != null);
            Assert.That(merge.Equals(branchHead), "Merge op should return head of current branch");
        }

        [Test]
        public void RepositoryBranch_Test_Merge_SourceAheadTarget()
        {
            var repository = new Repository<int, string>();
            var masterBranch = repository.CurrentBranch;
            var initialCommit = masterBranch.Commit(0, "initial value");
            var secondBranch = repository.CreateBranch("second");
            Assert.That(secondBranch.Head.Equals(initialCommit), "After creation Branch should point to the parent Head");

            masterBranch.Commit(1, "second commit");
            var masterHead = masterBranch.Head;

            var merge = secondBranch.MergeWith(masterBranch);
            Assert.That(merge != null);
            Assert.That(merge.Equals(masterHead), "Merge op should return head of master branch");
        }

        [Test]
        public void RepositoryBranch_Test_Merge_DeviatedBranches()
        {
            var repository = new Repository<int, string>();
            var masterBranch = repository.CurrentBranch;
            var initialCommit = masterBranch.Commit(0, "initial value");

            var secondBranch = repository.CreateBranch("second");
            Assert.That(secondBranch.Head.Equals(initialCommit), "After creation Branch should point to the parent Head");

            for (int i = 1; i <= 10; i++)
                masterBranch.Commit(i, i + " commit");
            var masterHead = masterBranch.Head;

            for (int i = 2; i <= 10; i += 2)
                secondBranch.Commit(i, i + " branch commit");
            var branchHead = secondBranch.Head;

            var merge = secondBranch.MergeWith(masterBranch);
            Assert.That(secondBranch.GetHistory().Count() == 7, "There should be 3 commits in second branch");

            Assert.That(merge != null);
            Assert.That(!merge.Equals(masterHead), "Merge op should return new commit");
            Assert.That(!merge.Equals(branchHead), "Merge op should return new commit");

            Assert.That(merge.Parents.First().Equals(branchHead), "Merge commit should have second branch head as a first parent");
            Assert.That(merge.Parents.Last().Equals(masterHead), "Merge commit should have master branch head as a second parent");
        }
    }

}