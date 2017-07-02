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
using CollabEdit.Services;

namespace CollabEdit.Controllers
{
    [ExplorerActionFilter]
    [Route("api/explorer")]
    public class ExplorerDataController : Controller
    {
        private readonly ILogger _logger;
        private readonly IPathMapService _pathMap;

        public ExplorerDataController(ILogger<ExplorerDataController> logger, IPathMapService pathMapService)
        {
            _logger = logger;
            _pathMap = pathMapService;
        }

        [HttpGet("folder/{*targetPath}", Name = "GetFolder")]
        public IActionResult GetFolderContent([FromRoute] string targetPath)
        {
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
        public IActionResult CreateFolder([FromRoute] string targetPath, [FromBody] string name)
        {
            string folderPath = FilePath.Combine(_pathMap.ToLocalPath(targetPath), name);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return CreatedAtRoute("GetFolder", new { targetPath = _pathMap.ToVirtulPath(folderPath) },
                new FileSystemInfoDto()
                {
                    Name = name,
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
            var statusResult = new List<ActionStausResponseDto<FileSystemInfoDto>>();
            var basePath = _pathMap.ToLocalPath(targetPath);
            foreach (var fsObject in entries)
            {
                if (string.IsNullOrEmpty(fsObject.Name))
                {
                    statusResult.Add(new ActionStausResponseDto<FileSystemInfoDto>
                    {
                        Entity = fsObject,
                        Status = ActionStatusResult.ClientFauilure,
                        Errros = new string[1] { "'name' may not be null or empty string" }
                    }
                    );
                    continue;
                }

                var path = FilePath.Combine(basePath, fsObject.Name);
                if (fsObject.IsFile && System.IO.File.Exists(path))
                {
                    var response = TryPerformActionOnEntity(() => System.IO.File.Delete(path), fsObject);
                    statusResult.Add(response);
                }

                if (!fsObject.IsFile && Directory.Exists(path))
                {
                    var response = TryPerformActionOnEntity(() => Directory.Delete(path, true), fsObject);
                    statusResult.Add(response);
                }
            }
            var actionResult = new ObjectResult(statusResult);

            if (statusResult.Any((item) => item.Status != ActionStatusResult.Ok))
                actionResult.StatusCode = 207; // multi status result

            return actionResult;
        }

        private ActionStausResponseDto<TObj> TryPerformActionOnEntity<TObj>(Action action, TObj entity)
        {
            try
            {
                action.Invoke();
                return new ActionStausResponseDto<TObj>(entity);
            }
            catch (IOException ex)
            {
                return new ActionStausResponseDto<TObj>(entity, ActionStatusResult.ServerFailure, ex);
            }
        }

        [HttpGet("file/{*targetPath}", Name = "GetFile")]
        public IActionResult GetFileContent([FromRoute] string targetPath)
        {
            var path = _pathMap.ToLocalPath(targetPath);
            return new FileStreamResult(System.IO.File.OpenRead(path), "text/plain");
        }

        [HttpPost("file/{*targetPath}", Name = "CreateFile")]
        public IActionResult Createfile([FromRoute] string targetPath, [FromBody] string name)
        {
            string fullPath = FilePath.Combine(_pathMap.ToLocalPath(targetPath), name);
            //We create file only if it does not exists; existing file is Ok since we do not provide any content at this point
            if (!System.IO.File.Exists(fullPath))
                using (System.IO.File.Create(fullPath)) { }

            return new CreatedAtRouteResult("GetFile", new { targetPath = _pathMap.ToVirtulPath(fullPath) },
            new FileSystemInfoDto()
            {
                Name = name,
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