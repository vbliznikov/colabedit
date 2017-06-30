using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using CollabEdit.Model;
using CollabEdit.IO;

namespace CollabEdit.Controllers
{
    [Route("api/explorer")]
    public class ExplorerDataController : Controller
    {
        private readonly ILogger _logger;
        private ExplorerOptions _configOptions;
        private const string contentRootPathdefault = "./wwwwroot/editor-root";
        private readonly PathMap _pathMap;

        public ExplorerDataController(ILogger<ExplorerDataController> logger, IOptions<ExplorerOptions> options)
        {
            _logger = logger;
            _configOptions = options.Value;
            string rootPath;

            if (string.IsNullOrEmpty(_configOptions.EditorRoot))
            {
                _logger.LogInformation("Editor root is not configured, use default value '{0}'", contentRootPathdefault);
                rootPath = contentRootPathdefault;
            }
            else
            {
                _logger.LogInformation("Editor root is configured to '{0}'", _configOptions.EditorRoot);
                rootPath = _configOptions.EditorRoot;
            }
            // Try to create folder
            if (!Directory.Exists(rootPath))
            {
                try
                {
                    Directory.CreateDirectory(rootPath);
                }
                catch (IOException ex)
                {
                    var msg = string.Format("Can't initialize Editor root folder '{0}'", Path.GetFullPath(rootPath));
                    throw new Exception(msg, ex);
                }
            }
            _pathMap = new PathMap(rootPath);
        }

        [HttpGet("folder/{*targetPath}", Name = "GetFolder")]
        public IActionResult GetFolderContent([FromRoute] string targetPath)
        {
            _logger.LogDebug("target path={0}", targetPath);
            if (string.IsNullOrEmpty(targetPath))
                targetPath = "home";

            var dirInfo = new DirectoryInfo(_pathMap.ToLocalPath(targetPath));
            var result = dirInfo.EnumerateFileSystemInfos().Select(value => new FileSystemInfoDto()
            {
                Name = value.Name,
                Path = _pathMap.ToVirtulPath(value.FullName),
                IsFile = !value.Attributes.HasFlag(FileAttributes.Directory)
            }).ToList();
            result.Sort();
            return new ObjectResult(result);
        }

        [HttpPost("folder/{*targetPath}", Name = "CreateFolder")]
        public IActionResult CreateFolder([FromRoute] string targetPath, [FromForm] string folderName)
        {
            string folderPath = FilePath.Combine(_pathMap.ToLocalPath(targetPath), folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return CreatedAtRoute("GetFolder", new { targetPath = _pathMap.ToVirtulPath(folderPath) },
                new FileSystemInfoDto()
                {
                    Name = folderName,
                    Path = _pathMap.ToVirtulPath(folderPath)
                });
        }

        [HttpDelete("file/{*targetPath}", Name = "DeleteFolder")]
        public IActionResult DeleteFolder([FromRoute] string targetPath)
        {
            var fullPath = _pathMap.ToLocalPath(targetPath);
            if (targetPath.Equals(_pathMap.VirtualRoot, StringComparison.OrdinalIgnoreCase)
                    || fullPath.Equals(_pathMap.PhysicalRoot, StringComparison.OrdinalIgnoreCase))
                return new StatusCodeResult(403);

            var dirInfo = new DirectoryInfo(fullPath);
            if (dirInfo.Exists)
                dirInfo.Delete(true);

            return new NoContentResult();
        }

        [HttpDelete("folder/{*targetPath}", Name = "DeleteFileSystemObjects")]
        public IActionResult DeleteFileSystemObjects([FromRoute] string targetPath, [FromBody] FileSystemInfoDto[] entries)
        {
            var basePath = _pathMap.ToLocalPath(targetPath);
            foreach (var fsObject in entries)
            {
                var path = FilePath.Combine(basePath, fsObject.Name);
                if (fsObject.IsFile && System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                if (!fsObject.IsFile && Directory.Exists(path))
                    Directory.Delete(path, true);
            }

            return new NoContentResult();
        }

        [HttpGet("file/{*targetPath}", Name = "GetFile")]
        public IActionResult GetFileContent([FromRoute] string targetPath)
        {
            var path = _pathMap.ToLocalPath(targetPath);
            return new FileStreamResult(System.IO.File.OpenRead(path), "text/plain");
        }

        [HttpPost("file/{*targetPath}", Name = "CreateFile")]
        public IActionResult Createfile([FromRoute] string targetPath, [FromForm] string fileName)
        {
            string fullPath = FilePath.Combine(_pathMap.ToLocalPath(targetPath), fileName);
            //We create file only if it does not exists; existing file is Ok since we do not provide any content at this point
            if (!System.IO.File.Exists(fullPath))
                using (System.IO.File.Create(fullPath)) { }

            return new CreatedAtRouteResult("GetFile", new { targetPath = _pathMap.ToVirtulPath(fullPath) },
            new FileSystemInfoDto()
            {
                Name = fileName,
                Path = _pathMap.ToVirtulPath(fullPath),
                IsFile = true
            });
        }

        [HttpPut("file/{*targetPath}", Name = "UpdateFile")]
        public async Task<IActionResult> UpdateFileContent([FromRoute] string targetPath, [FromBody] FileSystemInfoDto fsEntry)
        {
            string filePath = _pathMap.ToLocalPath(targetPath);
            //Handle only content update now;
            using (StreamWriter writer = new StreamWriter(System.IO.File.OpenWrite(filePath)))
                await writer.WriteAsync(fsEntry.Content);

            return new NoContentResult();
        }

        [HttpDelete("file/{*targetPath}", Name = "DeleteFile")]
        public IActionResult DeleteFile([FromRoute] string targetPath)
        {
            var path = _pathMap.ToLocalPath(targetPath);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            return new NoContentResult();
        }
    }

}