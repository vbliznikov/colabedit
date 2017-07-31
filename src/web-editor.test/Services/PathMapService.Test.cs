using System;
using NUnit.Framework;
using System.IO;
using Microsoft.Extensions.Options;

using CollabEdit.Services;
using CollabEdit.IO;

namespace CollabEdit.Test
{
    [TestFixture]
    public class PathMapTest
    {


        private IPathMapService GetPathMapService(ExplorerOptions config)
        {
            return new PathMapService(Options.Create(config));
        }
        private IPathMapService GetPathMapService(string localRoot, string virtulRoot = "home")
        {
            var config = new ExplorerOptions
            {
                EditorRoot = localRoot,
                VirtualRoot = virtulRoot
            };

            return new PathMapService(Options.Create(config));
        }

        [Test]
        public void CtorWithNullPathArgument_ShouldThrowsArgumentException()
        {
            Assert.That(() => new PathMapService(null), Throws.ArgumentNullException);
        }

        [Test]
        public void CtorWithNullConfigValue_ShouldUSeDefaultOptions()
        {
            var defaultOptions = new ExplorerOptions();
            var pathMap = new PathMapService(Options.Create<ExplorerOptions>(null));
            Assert.That(pathMap.PhysicalRoot, Is.EqualTo(Path.GetFullPath(defaultOptions.EditorRoot)));
            Assert.That(pathMap.VirtualRoot, Is.EqualTo(defaultOptions.VirtualRoot));
        }

        [Test]
        public void CtorWithExplicitNullConfigParams_ShouldTrhowException()
        {
            var badPathConfig = new ExplorerOptions { EditorRoot = "" };
            var badVirtualRootConfig = new ExplorerOptions { VirtualRoot = "" };

            Assert.That(() => new PathMapService(Options.Create(badPathConfig)), Throws.ArgumentException);
            Assert.That(() => new PathMapService(Options.Create(badVirtualRootConfig)), Throws.ArgumentException);
            // Assert.Throws<ArgumentException>("options",
            //     () => new PathMapService(Options.Create(badPathConfig)));
            // Assert.Throws<ArgumentException>("options",
            //     () => new PathMapService(Options.Create(badVirtualRootConfig)));
        }

        [Test]
        public void Ctor_ShouldCreateEdiorRoot_IfNotExists()
        {
            var rootPath = FilePath.Combine(Directory.GetCurrentDirectory(), "../../../editor-root1");
            if (Directory.Exists(rootPath))
                Directory.Delete(rootPath, true);

            var config = new ExplorerOptions
            {
                EditorRoot = rootPath,
                CreateIfNotExists = true
            };
            GetPathMapService(config);

            Assert.That(Directory.Exists(rootPath));
            Directory.Delete(rootPath);
        }

        [Test]
        public void CtorWithRootedPathConfig_ShouldPreservePath()
        {
            var path = "/home";
            var pathMap = GetPathMapService(path);

            Assert.That(pathMap.PhysicalRoot, Is.EqualTo(path));
        }

        [Test]
        public void CtorwithRelativePathConfig_ShouldExpand()
        {
            var path = "./home";
            var pathMap = GetPathMapService(path);

            Assert.That(pathMap.PhysicalRoot, Is.EqualTo(Path.GetFullPath(path)));
        }

        [Test]
        public void ToLocalPath_ShouldMapHomeToRoot()
        {
            var vPath = "home";
            var rootPath = "/root";

            var pathMap = GetPathMapService(rootPath, vPath);
            Assert.That(pathMap.ToLocalPath(vPath), Is.EqualTo(rootPath));
        }

        [Test]
        public void ToLocalPath_ShouldMapSubsequentParts()
        {
            var rootPath = "/root";
            var vPath = "home/file1/file2";
            var expectedPath = "/root/file1/file2";

            var pathMap = GetPathMapService(rootPath); //new PathMapService(rootPath);

            Assert.That(pathMap.ToLocalPath(vPath), Is.EqualTo(expectedPath));
        }

        [Test]
        public void ToLocalPath_ShouldMapPath_NotStartingWithHome()
        {
            var rootPath = "/root";
            var vPath = "dir1/dir2";
            var expectedPath = "/root/dir1/dir2";

            var pathMap = GetPathMapService(rootPath); //new PathMapService(rootPath);

            Assert.That(pathMap.ToLocalPath(vPath), Is.EqualTo(expectedPath));
        }

        [Test]
        public void ToVirtualPath_ShouldMapRootToHome()
        {
            var rootPath = "/root";
            var pPath = rootPath;
            var expectedPath = "home";

            var pathMap = GetPathMapService(rootPath); //new PathMapService(rootPath);

            Assert.That(pathMap.ToVirtulPath(pPath), Is.EqualTo(expectedPath));
        }

        [Test]
        public void ToVirtulPath_ShoulTrimRoot()
        {
            var rootPath = "/root";
            var additionalPath = "dir1/dir2";
            var pPath = string.Format("{0}/{1}", rootPath, additionalPath);
            var expectedPath = string.Format("home/{0}", additionalPath);

            var pathMap = GetPathMapService(rootPath); //new PathMapService(rootPath);

            Assert.That(pathMap.ToVirtulPath(pPath), Is.EqualTo(expectedPath));
        }
    }
}