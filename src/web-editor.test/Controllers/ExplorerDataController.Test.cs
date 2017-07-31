using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;

using CollabEdit.Controllers;
using CollabEdit.Services;
using CollabEdit.Model;
using CollabEdit.IO;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;

namespace CollabEdit.Controllers.Test
{
    [TestFixture]
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

        [Test]
        public void GetFolderContent_EmptyFolderIsEmpty()
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);
                var actionResult = controller.GetFolderContent("home");
                Assert.That(actionResult, Is.AssignableFrom<ObjectResult>());
                var result = (ObjectResult)actionResult;

                Assert.NotNull(result?.Value);
                Assert.That(result.Value, Is.AssignableTo<IEnumerable<FileSystemInfoDto>>());
                var enumerable = (IEnumerable<FileSystemInfoDto>)result.Value;
                Assert.That(enumerable.Count(), Is.EqualTo(0));
            });
        }


        [TestCase("home")]
        [TestCase("home/folder1")]
        [TestCase("home/folder1/folder2")]
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
                var actionResult = controller.GetFolderContent(vPath);
                Assert.That(actionResult, Is.AssignableFrom<ObjectResult>());
                var result = (ObjectResult)actionResult;

                Assert.NotNull(result?.Value);
                Assert.That(result.Value, Is.AssignableFrom<List<FileSystemInfoDto>>());
                var list = (List<FileSystemInfoDto>)result.Value;

                // Check total objects 
                Assert.That(list.Count, Is.EqualTo(foldersCount + filesCount));
                Assert.That(list.FindAll(item => !item.IsFile).Count, Is.EqualTo(foldersCount));
                Assert.That(list.FindAll(item => item.IsFile).Count, Is.EqualTo(filesCount));

                var folders = list.GetRange(0, foldersCount);
                var files = list.GetRange(foldersCount, filesCount);
                // Check that folders goes before files
                Assert.That(folders.Any((item) => item.IsFile), Is.False);
                Assert.That(files.Any((item) => !item.IsFile), Is.False);
                // Objects are sorted asc by name
                Assert.That(files[0], Is.LessThan(files[filesCount - 1]));
                // Assert.True(((IComparable)files[0]).CompareTo(files[filesCount - 1]) < 0);

            });
        }

        [TestCase("home")]
        [TestCase("home/folder1/folder2")]
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
                var actionResult = controller.GetFolderContent(vPath);
                Assert.That(actionResult, Is.AssignableFrom<ObjectResult>());
                var result = (ObjectResult)actionResult;

                Assert.NotNull(result?.Value);
                Assert.That(result.Value, Is.AssignableFrom<List<FileSystemInfoDto>>());
                var list = (List<FileSystemInfoDto>)result.Value;
                Assert.That(list.Count, Is.EqualTo(2));

                //Check folder, it should be first
                var folderDto = list[0];
                Assert.That(folderDto.IsFile, Is.False);
                Assert.That(folderDto.Name, Is.EqualTo(foldername));
                Assert.That(folderDto.Path, Is.EqualTo(string.Format("{0}/{1}", vPath, foldername)));

                //Check file, it should be second
                var fileDto = list[1];
                Assert.That(fileDto.IsFile);
                Assert.That(fileDto.Name, Is.EqualTo(fileName));
                Assert.That(fileDto.Path, Is.EqualTo(string.Format("{0}/{1}", vPath, fileName)));

            });
        }

        [TestCase("home")]
        [TestCase("home/folder2/folder-X")]
        public void CreateFolder_FolderDoesNotExists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFolderName = "folderNew";

                var actionResult = controller.CreateFolder(vPath, newFolderName);
                Assert.That(actionResult, Is.AssignableFrom<CreatedAtRouteResult>(), "actionResult is not ssignable from <CreatedAtRouteResult>");
                var result = (CreatedAtRouteResult)actionResult;

                // folder has been created
                Assert.That(currentFolder.FolderExists(newFolderName), "New Folder does not  exists");
                // route param is set correctly
                var expectedRouteParam = string.Format("{0}/{1}", vPath, newFolderName);
                Assert.That(result.RouteValues["targetPath"], Is.EqualTo(expectedRouteParam), "Route path is incorrect");
                // correct entry returned
                Assert.That(result.Value, Is.AssignableFrom<FileSystemInfoDto>(), "value returned is not assignable from <FileSystemInfoDto>");
                var dto = (FileSystemInfoDto)result.Value;
                Assert.That(dto.Name, Is.EqualTo(newFolderName), "dto.Name is not setup corretcly");
                Assert.That(dto.Path, Is.EqualTo(expectedRouteParam), "dto.Path is not setup correctly");
                Assert.That(dto.IsFile, Is.False, "dto.IsFile should be false");

            });
        }

        [TestCase("home")]
        [TestCase("home/folder2/folder-X")]
        public void CreateFolder_ShouldReturnOkForExistingFolder(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFolderName = "folderNew";
                currentFolder.CreateFolder(newFolderName);
                Assert.That(currentFolder.FolderExists(newFolderName));

                var actionResult = controller.CreateFolder(vPath, newFolderName);
                Assert.That(actionResult, Is.AssignableFrom<CreatedAtRouteResult>());
                var result = (CreatedAtRouteResult)actionResult;

                // route param is set correctly
                var expectedRouteParam = string.Format("{0}/{1}", vPath, newFolderName);
                Assert.That(result.RouteValues["targetPath"], Is.EqualTo(expectedRouteParam));
                // correct entry returned
                Assert.That(result.Value, Is.AssignableFrom<FileSystemInfoDto>());
                var dto = (FileSystemInfoDto)result.Value;
                Assert.That(dto.Name, Is.EqualTo(newFolderName));
                Assert.That(dto.Path, Is.EqualTo(expectedRouteParam));
                Assert.That(dto.IsFile, Is.False);

            });
        }

        [TestCase("home/folder1")]
        [TestCase("home/folder2/folder-X")]
        public void DeleteFolder_EmptyFolderExists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFolderName = "folderNew";
                currentFolder.CreateFolder(newFolderName);
                Assert.That(currentFolder.FolderExists(newFolderName));

                var actionResult = controller.DeleteFolder(FilePath.Combine(vPath, newFolderName));
                Assert.That(actionResult, Is.AssignableFrom<NoContentResult>());
                var result = (NoContentResult)actionResult;

                Assert.That(result.StatusCode, Is.EqualTo(204));
                Assert.That(currentFolder.FolderExists(newFolderName), Is.False);

            });
        }

        [TestCase("home/folder1")]
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
                Assert.That(currentFolder.FolderExists(newFolderName));
                Assert.That(currentFolder.FolderIsEmpty(newFolderName), Is.False);

                var actionResult = controller.DeleteFolder(FilePath.Combine(vPath, newFolderName));
                Assert.That(actionResult, Is.AssignableFrom<NoContentResult>());
                var result = (NoContentResult)actionResult;

                Assert.That(result.StatusCode, Is.EqualTo(204));
                Assert.That(currentFolder.FolderExists(newFolderName), Is.False);

            });
        }

        [TestCase("home/folder1")]
        [TestCase("home/folder2/folder")]
        public void DeleteFolder_FolderDoesNotExists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFolderName = "folderNew";
                Assert.That(currentFolder.FolderExists(newFolderName), Is.False);

                var actionResult = controller.DeleteFolder(FilePath.Combine(vPath, newFolderName));
                Assert.That(actionResult, Is.AssignableFrom<NoContentResult>());
                var result = (NoContentResult)actionResult;

                Assert.That(result.StatusCode, Is.EqualTo(204));
                Assert.That(currentFolder.FolderExists(newFolderName), Is.False);

            });
        }

        [Test]
        public void DeleteFolder_FolderHomeCouldNotBeDeleted()
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = editorRoot;
                Assert.True(currentFolder.FolderIsEmpty());

                var actionResult = controller.DeleteFolder("home");
                Assert.That(actionResult, Is.AssignableFrom<StatusCodeResult>());
                var result = (StatusCodeResult)actionResult;
                // Delete root is forbidden
                Assert.That(result.StatusCode, Is.EqualTo(403));

            });
        }

        [TestCase("home")]
        [TestCase("home/folder2/folder")]
        public void CreateFile_WhichDoesNotEXists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFilename = "newFile.txt";
                // File does not exists
                Assert.False(currentFolder.FileExists(newFilename));

                var actionResult = controller.Createfile(vPath, newFilename);
                Assert.That(actionResult, Is.AssignableFrom<CreatedAtRouteResult>());
                var result = (CreatedAtRouteResult)actionResult;

                // File was created
                Assert.That(currentFolder.FileExists(newFilename));
                // route param is set correctly
                var expectedRouteParam = string.Format("{0}/{1}", vPath, newFilename);
                Assert.That(result.RouteValues["targetPath"], Is.EqualTo(expectedRouteParam));
                // correct entry returned
                Assert.That(result.Value, Is.AssignableFrom<FileSystemInfoDto>());
                var dto = (FileSystemInfoDto)result.Value;
                Assert.That(dto.Name, Is.EqualTo(newFilename));
                Assert.That(dto.Path, Is.EqualTo(expectedRouteParam));
                Assert.That(dto.IsFile);

            });
        }

        [TestCase("home/folder21/folder")]
        public void CreateFile_WichExistsDontOverrideContent(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFilename = "newFile22.txt";
                const string expectedContent = "some initial content";

                currentFolder.CreateFileWithContent(newFilename, expectedContent);
                Assert.That(currentFolder.FileExists(newFilename));

                var actionResult = controller.Createfile(vPath, newFilename);
                Assert.That(actionResult, Is.AssignableFrom<CreatedAtRouteResult>());
                var result = (CreatedAtRouteResult)actionResult;

                //Assert that file was not ovewritten
                Assert.That(currentFolder.FileExists(newFilename));
                var newContent = currentFolder.ReadFile(newFilename);
                Assert.That(newContent, Is.EqualTo(expectedContent));

                // correct entry returned
                Assert.That(result.Value, Is.AssignableFrom<FileSystemInfoDto>());
                var dto = (FileSystemInfoDto)result.Value;
                Assert.That(dto.Name, Is.EqualTo(newFilename));
                Assert.That(dto.Path, Is.EqualTo(string.Format("{0}/{1}", vPath, newFilename)));
                Assert.That(dto.IsFile);

            });
        }

        [TestCase("home")]
        [TestCase("home/folder22")]
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
                Assert.That(currentFolder.FileExists(newFilename));

                var actionResult = controller.GetFileContent(FilePath.Combine(vPath, newFilename));
                Assert.That(actionResult, Is.AssignableFrom<FileStreamResult>());
                var result = (FileStreamResult)actionResult;

                Assert.That(result.FileStream, Is.Not.Null);
                Assert.That(result.ContentType, Is.EqualTo("text/plain"));
                using (StreamReader reader = new StreamReader(result.FileStream))
                {
                    int linesRead = 0;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        linesRead++;
                        Assert.That(line, Is.EqualTo(string.Format(linePattern, linesRead)));
                    }
                    Assert.That(linesRead, Is.EqualTo(lineCount));
                }

            });
        }

        [TestCase("home")]
        public void GetFileContent_EmptyFileExists(string vPath)
        {
            RunTest(() =>
            {
                Assert.NotNull(controller);

                var currentFolder = SetupFoldersHierarchy(vPath);
                var newFilename = "newFile.txt";
                currentFolder.CreateFile(newFilename);
                Assert.That(currentFolder.FileExists(newFilename));

                var actionResult = controller.GetFileContent(FilePath.Combine(vPath, newFilename));
                Assert.That(actionResult, Is.AssignableFrom<FileStreamResult>());
                FileStreamResult result = (FileStreamResult)actionResult;

                Assert.That(result.FileStream, Is.Not.Null);
                Assert.That(result.ContentType, Is.EqualTo("text/plain"));
                using (StreamReader reader = new StreamReader(result.FileStream))
                {
                    Assert.True(reader.EndOfStream);
                }

            });
        }

        [TestCase("home")]
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
                Assert.That(currentFolder.FileExists(filename));

                var task = controller.UpdateFileContent(targetPath, fsEntry);
                task.Wait();
                Assert.IsAssignableFrom<NoContentResult>(task.Result);
                var typedResult = task.Result;

                Assert.That(currentFolder.FileExists(filename));
                Assert.That(expectedContent, Is.EqualTo(currentFolder.ReadFile(filename)));

            });
        }
    }
}