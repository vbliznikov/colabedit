using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace CollabEdit.VersionControl.Tests
{
    [TestFixture]
    public class TestRepositoryBranchInternals : RepositoryBranch<int, string>
    {
        private Commit<int, string> CreateCommit(int value, string comment, params Commit<int, string>[] parents)
        {
            return new Commit<int, string>(value, comment, parents);
        }

        [Test]
        public void Test_FindCommonAncestor_AfterMerge()
        {
            //     .1
            //  .2/ \
            //.4/--->.3
            //        \.5

            var root = CreateCommit(1, "root");
            var commit2 = CreateCommit(2, "commit2", root);
            var commit3 = CreateCommit(2, "commit3", root);
            var commit4 = CreateCommit(3, "commit4", commit2, commit3);
            var commit5 = CreateCommit(3, "commit5", commit3);

            var lca = this.FindCommonAcestor(commit4, commit5);
            Assert.AreEqual(commit3, lca, "The LCA should be commit3");
        }

        [Test]
        public void Test_FindCommonAncestor_AfterTwoMerges()
        {
            //               .1
            //            .2/|3\.4
            //          .5/<-|6 \.7
            //         .8/   |9/

            var root = CreateCommit(0, "root");
            var commit2 = CreateCommit(1, "commit2", root);
            var commit3 = CreateCommit(1, "commit3", root);
            var commit4 = CreateCommit(1, "commit4", root);
            var commit5 = CreateCommit(2, "commit5", commit2);
            var commit6 = CreateCommit(2, "commit6", commit5, commit3);
            var commit7 = CreateCommit(2, "commit7", commit4);
            var commit8 = CreateCommit(3, "commit8", commit5);
            var commit9 = CreateCommit(3, "commit9", commit6, commit7);

            var lca = FindCommonAcestor(commit8, commit9);
            Assert.AreEqual(commit5, lca, "The LCA should be comment5!");

            lca = FindCommonAcestor(commit9, commit4);
            Assert.AreEqual(commit4, lca, "The LCA should be commit4");
        }

        [Test]
        public void Test_FindCommonAncestor_DiamondLeftCase()
        {
            //           .1
            //        2./ \.3
            //       4./\.5
            //         \/
            //          6

            var root = CreateCommit(0, "root");
            var commit2 = CreateCommit(1, "commit2", root);
            var commit3 = CreateCommit(1, "commit3", root);
            var commit4 = CreateCommit(2, "commit4", commit2);
            var commit5 = CreateCommit(2, "commit5", commit2);
            var commit6 = CreateCommit(3, "commit6", commit4, commit5);

            var lca = FindCommonAcestor(commit6, commit3);
            Assert.AreEqual(root, lca, "The LCA shoould be Root");
        }

        [Test]
        public void Test_FindCommonAncestor_DiamondRightCase()
        {
            //           .1
            //        2./ \.3
            //          4./\.5
            //            \/
            //             6

            var root = CreateCommit(0, "root");
            var commit2 = CreateCommit(1, "commit2", root);
            var commit3 = CreateCommit(1, "commit3", root);
            var commit4 = CreateCommit(2, "commit4", commit3);
            var commit5 = CreateCommit(2, "commit5", commit3);
            var commit6 = CreateCommit(3, "commit6", commit4, commit5);

            var lca = FindCommonAcestor(commit6, commit2);
            Assert.AreEqual(root, lca, "The LCA shoould be Root");
        }
    }
}