using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using CollabEdit.Model;

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
        public IActionResult CreateFolder([FromRoute] string targetPath, [FromForm] string name)
        {
            string folderPath = Path.Combine(_pathMap.ToLocalPath(targetPath), name);
            Directory.CreateDirectory(folderPath);

            return CreatedAtRoute("GetFolder", new { targetPath = _pathMap.ToVirtulPath(folderPath) },
                new FileSystemInfoDto()
                {
                    Name = name,
                    Path = _pathMap.ToVirtulPath(folderPath)
                });
        }

        [HttpDelete("folder/{*targetPath}", Name = "DeleteFileSystemObjects")]
        public IActionResult DeleteFileSystemObjects([FromRoute] string targetPath, [FromBody] FileSystemInfoDto[] objects)
        {
            var basePath = _pathMap.ToLocalPath(targetPath);
            foreach (var fsObject in objects)
            {
                var path = Path.Combine(basePath, fsObject.Name);
                if (fsObject.IsFile && System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                if (!fsObject.IsFile && Directory.Exists(path))
                    Directory.Delete(path);
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
        public IActionResult Createfile([FromRoute] string targetPath, [FromForm] string name)
        {
            string fullPath = Path.Combine(_pathMap.ToLocalPath(targetPath), name);
            using (System.IO.File.Create(fullPath)) { }

            return new CreatedAtRouteResult("GetFile", new { targetPath = _pathMap.ToVirtulPath(fullPath) },
            new FileSystemInfoDto()
            {
                Name = name,
                Path = _pathMap.ToVirtulPath(fullPath),
                IsFile = true
            });
        }

        [HttpDelete("file/{*targetPath}", Name = "DelteFile")]
        public IActionResult DeleteFile([FromRoute] string targetPath)
        {
            var path = _pathMap.ToLocalPath(targetPath);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            return new NoContentResult();
        }
    }

}