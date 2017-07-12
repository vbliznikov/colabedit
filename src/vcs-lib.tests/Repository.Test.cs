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
    }

}