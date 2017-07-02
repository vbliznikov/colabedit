using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

using CollabEdit.Controllers;
using CollabEdit.Services;
using CollabEdit.Model;
using CollabEdit.IO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;

namespace CollabEdit.Controllers.Test
{
    public class ExplorerDataControllerTest
    {
        private ILogger<ExplorerDataController> _logger;
        private IPathMapService _pathMapService;
        private const string contentRoot = "../../../editor-root";
        internal FolderUtil editorRoot;
        protected ExplorerDataController controller;
        private static Semaphore runningTestsSemaphor = new Semaphore(1, 1);

        public ExplorerDataControllerTest()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole();
            _logger = loggerFactory.CreateLogger<ExplorerDataController>();

            var config = new ExplorerOptions
            {
                EditorRoot = FilePath.Combine(Directory.GetCurrentDirectory(), contentRoot),
                CreateIfNotExists = true
            };
            _pathMapService = new PathMapService(Options.Create(config));
            editorRoot = new FolderUtil(config.EditorRoot);
        }
        protected void Setup()
        {
            if (!runningTestsSemaphor.WaitOne(100))
                throw new TimeoutException("Can't enter semaphor");
            editorRoot.Clear();
            controller = new ExplorerDataController(_logger, _pathMapService);
        }

        protected void TearDown()
        {
            try
            {
                editorRoot.Clear();
            }
            catch { }
            runningTestsSemaphor.Release();
        }

        protected void RunTest(Action test)
        {
            Setup();
            try
            {
                test.Invoke();
            }
            finally
            {
                TearDown();
            }
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
            RunTest(() =>
            {
                Assert.NotNull(controller);
                ObjectResult result = Assert.IsAssignableFrom<ObjectResult>(controller.GetFolderContent("home"));
                Assert.NotNull(result?.Value);
                var enumerable = Assert.IsAssignableFrom<IEnumerable<FileSystemInfoDto>>(result.Value);
                Assert.Equal(0, enumerable.Count());

            });
        }

        [Theory]
        [InlineData("home")]
        [InlineData("home/folder1")]
        [InlineData("home/folder1/folder2")]
        public void GetFolderContent_NonEmptyFolderGeneral(string vPath)
        {
            RunTest(() =>
            {

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

            });
        }

        [Theory]
        [InlineData("home")]
        [InlineData("home/folder1/folder2")]
        public void GetFolderContent_CorrectEntryValuesReturned(string vPath)
        {
            RunTest(() =>
            {

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

            });
        }

        [Theory]
        [InlineData("home")]
        [InlineData("home/folder2/folder-X")]
        public void CreateFolder_FolderDoesNotExists(string vPath)
        {
            RunTest(() =>
            {
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

            });
        }

        [Theory]
        [InlineData("home")]
        [InlineData("home/folder2/folder-X")]
        public void CreateFolder_ShouldReturnOkForExistingFolder(string vPath)
        {
            RunTest(() =>
            {
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

            });
        }

        [Theory]
        [InlineData("home/folder1")]
        [InlineData("home/folder2/folder-X")]
        public void DeleteFolder_EmptyFolderExists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFolderName = "folderNew";
                currentFolder.CreateFolder(newFolderName);
                Assert.True(currentFolder.FolderExists(newFolderName));

                var result = Assert.IsAssignableFrom<NoContentResult>(controller.DeleteFolder(FilePath.Combine(vPath, newFolderName)));
                Assert.Equal(204, result.StatusCode);
                Assert.False(currentFolder.FolderExists(newFolderName));

            });
        }

        [Theory]
        [InlineData("home/folder1")]
        public void DeleteFolder_NonEmptyFolderExists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFolderName = "folderNew";
                currentFolder
                    .CreateFolder(newFolderName)
                    .CreateFiles(2)
                    .CreateFolders(1);
                Assert.True(currentFolder.FolderExists(newFolderName));
                Assert.False(currentFolder.FolderIsEmpty(newFolderName));

                var result = Assert.IsAssignableFrom<NoContentResult>(controller.DeleteFolder(FilePath.Combine(vPath, newFolderName)));
                Assert.Equal(204, result.StatusCode);
                Assert.False(currentFolder.FolderExists(newFolderName));

            });
        }

        [Theory]
        [InlineData("home/folder1")]
        [InlineData("home/folder2/folder")]
        public void DeleteFolder_FolderDoesNotExists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFolderName = "folderNew";
                Assert.False(currentFolder.FolderExists(newFolderName));

                var result = Assert.IsAssignableFrom<NoContentResult>(controller.DeleteFolder(FilePath.Combine(vPath, newFolderName)));
                Assert.Equal(204, result.StatusCode);
                Assert.False(currentFolder.FolderExists(newFolderName));

            });
        }

        [Fact]
        public void DeleteFolder_FolderHomeCouldNotBeDeleted()
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = editorRoot;
                Assert.True(currentFolder.FolderIsEmpty());

                var result = Assert.IsAssignableFrom<StatusCodeResult>(controller.DeleteFolder("home"));
                // Delete root is forbidden
                Assert.Equal(403, result.StatusCode);

            });
        }

        [Theory]
        [InlineData("home")]
        [InlineData("home/folder2/folder")]
        public void CreateFile_WhichDoesNotEXists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFilename = "newFile.txt";
                // File does not exists
                Assert.False(currentFolder.FileExists(newFilename));

                var result = Assert.IsAssignableFrom<CreatedAtRouteResult>(controller.Createfile(vPath, newFilename));
                // File was created
                Assert.True(currentFolder.FileExists(newFilename));
                // route param is set correctly
                var expectedRouteParam = string.Format("{0}/{1}", vPath, newFilename);
                Assert.Equal(expectedRouteParam, result.RouteValues["targetPath"]);
                // correct entry returned
                var dto = Assert.IsAssignableFrom<FileSystemInfoDto>(result.Value);
                Assert.Equal(newFilename, dto.Name);
                Assert.Equal(expectedRouteParam, dto.Path);
                Assert.True(dto.IsFile);

            });
        }

        [Theory]
        [InlineData("home/folder21/folder")]
        public void CreateFile_WichExistsDontOverrideContent(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFilename = "newFile22.txt";
                const string expectedContent = "some initial content";

                currentFolder.CreateFileWithContent(newFilename, expectedContent);
                Assert.True(currentFolder.FileExists(newFilename));

                var result = Assert.IsAssignableFrom<CreatedAtRouteResult>(controller.Createfile(vPath, newFilename));
                //Assert that file was not ovewritten
                Assert.True(currentFolder.FileExists(newFilename));
                var newContent = currentFolder.ReadFile(newFilename);
                Assert.Equal(expectedContent, newContent);

                // correct entry returned
                var dto = Assert.IsAssignableFrom<FileSystemInfoDto>(result.Value);
                Assert.Equal(newFilename, dto.Name);
                Assert.Equal(string.Format("{0}/{1}", vPath, newFilename), dto.Path);
                Assert.True(dto.IsFile);

            });
        }

        [Theory]
        [InlineData("home")]
        [InlineData("home/folder22")]
        public void GetFileContent_NonEmptyFileExist(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFilename = "newFile.txt";
                System.Text.StringBuilder contentBuilder = new System.Text.StringBuilder();
                var linePattern = "at line{0}: some initial content";
                const int lineCount = 10;
                for (int i = 1; i <= lineCount; i++)
                {
                    contentBuilder.AppendFormat(linePattern, i);
                    contentBuilder.Append("\n");
                }

                currentFolder.CreateFileWithContent(newFilename, contentBuilder.ToString());
                Assert.True(currentFolder.FileExists(newFilename));

                var result = Assert.IsAssignableFrom<FileStreamResult>(controller.GetFileContent(FilePath.Combine(vPath, newFilename)));
                Assert.NotNull(result.FileStream);
                Assert.Equal("text/plain", result.ContentType);
                using (StreamReader reader = new StreamReader(result.FileStream))
                {
                    int linesRead = 0;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        linesRead++;
                        Assert.Equal(string.Format(linePattern, linesRead), line);
                    }
                    Assert.Equal(lineCount, linesRead);
                }

            });
        }

        [Theory]
        [InlineData("home")]
        public void GetFileContent_EmptyFileExists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFilename = "newFile.txt";
                currentFolder.CreateFile(newFilename);
                Assert.True(currentFolder.FileExists(newFilename));

                var result = Assert.IsAssignableFrom<FileStreamResult>(controller.GetFileContent(FilePath.Combine(vPath, newFilename)));
                Assert.NotNull(result.FileStream);
                Assert.Equal("text/plain", result.ContentType);
                using (StreamReader reader = new StreamReader(result.FileStream))
                {
                    Assert.True(reader.EndOfStream);
                }

            });
        }

        [Theory]
        [InlineData("home")]
        public void UpdateFileContent_EmptyFileExists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                const string filename = "some-file.txt";
                var expectedContent = new String('0', 1024 * 1024);
                var targetPath = FilePath.Combine(vPath, filename);
                var fsEntry = new FileSystemInfoDto()
                {
                    Name = filename,
                    Path = targetPath,
                    Content = expectedContent
                };

                currentFolder.CreateFile(filename);
                Assert.True(currentFolder.FileExists(filename));

                var task = controller.UpdateFileContent(targetPath, fsEntry);
                task.Wait();
                var typedResult = Assert.IsAssignableFrom<NoContentResult>(task.Result);

                Assert.True(currentFolder.FileExists(filename));
                Assert.Equal(expectedContent, currentFolder.ReadFile(filename));

            });
        }
    }
}