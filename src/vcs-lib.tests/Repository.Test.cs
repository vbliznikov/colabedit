using System;
using NUnit.Framework;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace VersionControl.Tests
{
    [TestFixture]
    public class TestRepository
    {

        [Test]
        public void Repositorty_Test_EmtyRepository_HasEmptyHistory()
        {
            var repository = new Repository<string>();

            Assert.That(repository.Head == null);
            Assert.That(repository.GetHistory().Count() == 0, "Empty repository should have zero length history");
        }

        [Test]
        public void Repository_Test_CommitOperation()
        {
            var repository = new Repository<string>();
            Assert.That(repository.Head == null);

            var commit = repository.Commit(string.Empty, string.Empty);
            Assert.That(repository.Head.Equals(commit), "Repo should poit to the last commit");
            Assert.That(commit.Parent == null, "First commit should not have any parents");

            var commit2 = repository.Commit("other", string.Empty);
            Assert.That(repository.Head.Equals(commit2), "Repo should poit to the last commit");
            Assert.That(commit2.Parent != null, "New commit should point to previous one");
            Assert.That(commit2.Parent.Equals(commit), "New commit should point to previous one");
            Assert.That(repository.GetHistory().Count() == 2, "Repository should contains exactly two commits at this point");
            Assert.That(repository.GetHistory().First().Equals(commit2), "History should starts from last commit");
        }

        [Test]
        public void Repository_Test_CommitTheSameValue()
        {
            var repository = new Repository<string>();
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
    }

}