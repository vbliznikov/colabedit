using System;
using Xunit;
using System.IO;

using CollabEdit.Controllers;

namespace CollabEdit.Test
{
    public class PathMapTest
    {
        [Fact]
        public void CtorWithNullPathArgument_ShouldThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>("physicalRoot", () => new PathMap(null));
        }

        [Fact]
        public void CtorWithRootedPatArgument_ShouldPreservePath()
        {
            var path = "/home";
            var pathMap = new PathMap(path);

            Assert.Equal(path, pathMap.PhysicalRoot);
        }

        [Fact]
        public void CtorwithRelativePathArgument_ShouldExpand()
        {
            var path = "./home";
            var pathMap = new PathMap(path);

            Assert.Equal(Path.GetFullPath(path), pathMap.PhysicalRoot);
        }

        [Fact]
        public void ToLocalPath_ShouldMapHomeToRoot()
        {
            var vPath = "home";
            var rootPath = "/root";

            var pathMap = new PathMap(rootPath);

            Assert.Equal(rootPath, pathMap.ToLocalPath(vPath));
        }

        [Fact]
        public void ToLocalPath_ShouldMapSubsequentParts()
        {
            var rootPath = "/root";
            var vPath = "home/file1/file2";
            var expectedPath = "/root/file1/file2";

            var pathMap = new PathMap(rootPath);

            Assert.Equal(expectedPath, pathMap.ToLocalPath(vPath));
        }

        [Fact]
        public void ToLocalPath_ShouldMapPath_NotStartingWithHome()
        {
            var rootPath = "/root";
            var vPath = "dir1/dir2";
            var expectedPath = "/root/dir1/dir2";

            var pathMap = new PathMap(rootPath);

            Assert.Equal(expectedPath, pathMap.ToLocalPath(vPath));
        }

        [Fact]
        public void ToVirtualPath_ShouldMapRootToHome()
        {
            var rootPath = "/root";
            var pPath = rootPath;
            var expectedPath = "home";

            var pathMap = new PathMap(rootPath);

            Assert.Equal(expectedPath, pathMap.ToVirtulPath(pPath));
        }

        [Fact]
        public void ToVirtulPath_ShoulTrimRoot()
        {
            var rootPath = "/root";
            var additionalPath = "dir1/dir2";
            var pPath = string.Format("{0}/{1}", rootPath, additionalPath);
            var expectedPath = string.Format("home/{0}", additionalPath);

            var pathMap = new PathMap(rootPath);

            Assert.Equal(expectedPath, pathMap.ToVirtulPath(pPath));
        }
    }
}