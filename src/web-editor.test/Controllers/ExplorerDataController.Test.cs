using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;

using CollabEdit.Controllers;
using CollabEdit.Model;
using CollabEdit.IO;
using Microsoft.AspNetCore.Mvc;

namespace CollabEdit.Controllers.Test
{
    public class ExplorerDataControllerTest
    {
        private ILoggerFactory _logegrFactory;
        private ILogger<ExplorerDataController> _logger;
        private const string contentRoot = "../../../editor-root";
        private IOptions<ExplorerOptions> _options;
        internal FolderUtil editorRoot;
        protected ExplorerDataController controller;

        public ExplorerDataControllerTest()
        {
            _logegrFactory = new LoggerFactory();
            _logegrFactory.AddConsole();
            _logger = _logegrFactory.CreateLogger<ExplorerDataController>();
            var mock = new Mock<IOptions<ExplorerOptions>>();
            mock.Setup(options => options.Value).Returns(new ExplorerOptions()
            {
                EditorRoot = FilePath.Combine(Directory.GetCurrentDirectory(), contentRoot)
            });
            _options = mock.Object;
            editorRoot = new FolderUtil(_options.Value.EditorRoot);
        }

        protected void Setup()
        {
            editorRoot.Clear();
            controller = new ExplorerDataController(_logger, _options);
        }
        protected FolderUtil SetupFoldersHierarchy(string vPath)
        {
            string[] pathParts = vPath.Split('/');
            var currentFolder = editorRoot;
            // part[0] should be home;
            for (int i = 1; i < pathParts.Length; i++)
                currentFolder = currentFolder.CreateFolder(pathParts[i]);

            return currentFolder;
        }

        [Fact]
        public void GetFolderContent_EmptyFolderIsEmpty()
        {
            Setup();

            Assert.NotNull(controller);
            ObjectResult result = Assert.IsAssignableFrom<ObjectResult>(controller.GetFolderContent("home"));
            Assert.NotNull(result?.Value);
            var enumerable = Assert.IsAssignableFrom<IEnumerable<FileSystemInfoDto>>(result.Value);
            Assert.Equal(0, enumerable.Count());
        }

        [Theory]
        [InlineData("home")]
        [InlineData("home/folder1")]
        [InlineData("home/folder1/folder2")]
        public void GetFolderContent_NonEmptyFolderGeneral(string vPath)
        {
            Setup();

            var currentFolder = SetupFoldersHierarchy(vPath);
            const int foldersCount = 2;
            const int filesCount = 3;
            currentFolder
                .CreateFolders(foldersCount)
                .CreateFiles(filesCount);

            Assert.NotNull(controller);
            ObjectResult result = Assert.IsAssignableFrom<ObjectResult>(controller.GetFolderContent(vPath));
            Assert.NotNull(result?.Value);
            var list = Assert.IsAssignableFrom<List<FileSystemInfoDto>>(result.Value);

            // Check total objects 
            Assert.Equal(foldersCount + filesCount, list.Count);
            Assert.Equal(foldersCount, list.FindAll(item => !item.IsFile).Count);
            Assert.Equal(filesCount, list.FindAll(item => item.IsFile).Count);

            var folders = list.GetRange(0, foldersCount);
            var files = list.GetRange(foldersCount, filesCount);
            // Check that folders goes before files
            Assert.DoesNotContain(folders, (item) => item.IsFile);
            Assert.DoesNotContain(files, (item) => !item.IsFile);
            // Objects are sorted asc by name
            Assert.True(((IComparable)files[0]).CompareTo(files[filesCount - 1]) < 0);
        }

        [Theory]
        [InlineData("home")]
        [InlineData("home/folder1/folder2")]
        public void GetFolderContent_CorrectEntryValuesReturned(string vPath)
        {
            Setup();

            var currentFolder = SetupFoldersHierarchy(vPath);
            const string foldername = "Myfolder";
            const string fileName = "myFile.txt";
            currentFolder.CreateFolder(foldername);
            currentFolder.CreateFile(fileName);

            Assert.NotNull(controller);
            ObjectResult result = Assert.IsAssignableFrom<ObjectResult>(controller.GetFolderContent(vPath));

            Assert.NotNull(result?.Value);
            var list = Assert.IsAssignableFrom<List<FileSystemInfoDto>>(result.Value);
            Assert.Equal(2, list.Count);

            //Check folder, it should be first
            var folderDto = list[0];
            Assert.False(folderDto.IsFile);
            Assert.Equal(foldername, folderDto.Name);
            Assert.Equal(string.Format("{0}/{1}", vPath, foldername), folderDto.Path);

            //Check file, it should be second
            var fileDto = list[1];
            Assert.True(fileDto.IsFile);
            Assert.Equal(fileName, fileDto.Name);
            Assert.Equal(string.Format("{0}/{1}", vPath, fileName), fileDto.Path);
        }

        [Theory]
        [InlineData("home")]
        [InlineData("home/folder2/folder-X")]
        public void CreateFolder_FolderDoesNotExists(string vPath)
        {
            Setup();
            Assert.NotNull(controller);

            var currentFolder = SetupFoldersHierarchy(vPath);
            var newFolderName = "folderNew";

            var result = Assert.IsAssignableFrom<CreatedAtRouteResult>(controller.CreateFolder(vPath, newFolderName));

            // folder has been created
            Assert.True(currentFolder.FolderExists(newFolderName));
            // route param is set correctly
            var expectedRouteParam = string.Format("{0}/{1}", vPath, newFolderName);
            Assert.Equal(expectedRouteParam, result.RouteValues["targetPath"]);
            // correct entry returned
            var dto = Assert.IsAssignableFrom<FileSystemInfoDto>(result.Value);
            Assert.Equal(newFolderName, dto.Name);
            Assert.Equal(expectedRouteParam, dto.Path);
            Assert.False(dto.IsFile);
        }

        [Theory]
        [InlineData("home")]
        [InlineData("home/folder2/folder-X")]
        public void CreateFolder_ShouldReturnOkForExistingFolder(string vPath)
        {
            Setup();
            Assert.NotNull(controller);

            var currentFolder = SetupFoldersHierarchy(vPath);
            var newFolderName = "folderNew";
            currentFolder.CreateFolder(newFolderName);
            Assert.True(currentFolder.FolderExists(newFolderName));

            var result = Assert.IsAssignableFrom<CreatedAtRouteResult>(controller.CreateFolder(vPath, newFolderName));

            // route param is set correctly
            var expectedRouteParam = string.Format("{0}/{1}", vPath, newFolderName);
            Assert.Equal(expectedRouteParam, result.RouteValues["targetPath"]);
            // correct entry returned
            var dto = Assert.IsAssignableFrom<FileSystemInfoDto>(result.Value);
            Assert.Equal(newFolderName, dto.Name);
            Assert.Equal(expectedRouteParam, dto.Path);
            Assert.False(dto.IsFile);
        }
    }
}