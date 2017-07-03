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
using CollabEdit.Filters;

namespace CollabEdit.Controllers
{
    [ApiExceptionFilter()]
    [Route("api/explorer")]
    public class ExplorerDataController : Controller
    {
        private readonly ILogger _logger;
        private readonly IPathMapService _pathMap;
        private const string apiErrorKey = "apiErrors";

        public ExplorerDataController(ILogger<ExplorerDataController> logger, IPathMapService pathMapService)
        {
            _logger = logger;
            _pathMap = pathMapService;
        }

        private void ValidateTargetPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            var parts = path.Split('/');
            if (!parts[0].Equals(_pathMap.VirtualRoot, StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(apiErrorKey, "'targetPath' should start from 'home'");
        }

        private void ValidateRequired(object paramValue, string paramName = "name")
        {
            if (paramValue == null || (paramValue is string && string.IsNullOrEmpty(paramName as string)))
                ModelState.AddModelError(apiErrorKey,
                    string.Format("'{0}' is required and should be provided.", paramName));
        }

        private string NormalizeTargetPath(string vPath)
        {
            return vPath.Trim('/', ' ');
        }

        [HttpGet("folder/{*targetPath}", Name = "GetFolder")]
        public IActionResult GetFolderContent([FromRoute] string targetPath)
        {
            ValidateRequired(targetPath, nameof(targetPath));
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            targetPath = NormalizeTargetPath(targetPath);
            ValidateTargetPath(targetPath);
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var localPath = _pathMap.ToLocalPath(targetPath);
            if (!Directory.Exists(localPath))
                return new NotFoundResult();

            var dirInfo = new DirectoryInfo(localPath);
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
            ValidateRequired(targetPath, nameof(targetPath));
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            targetPath = NormalizeTargetPath(targetPath);
            ValidateTargetPath(targetPath);
            ValidateRequired(name);
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var localPath = _pathMap.ToLocalPath(targetPath);
            if (!Directory.Exists(localPath))
                return new NotFoundResult();

            string folderPath = FilePath.Combine(localPath, name);
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
            ValidateRequired(targetPath, nameof(targetPath));
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            targetPath = NormalizeTargetPath(targetPath);
            ValidateTargetPath(targetPath);
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var localPath = _pathMap.ToLocalPath(targetPath);
            if (targetPath.Equals(_pathMap.VirtualRoot, StringComparison.OrdinalIgnoreCase)
                    || localPath.Equals(_pathMap.PhysicalRoot, StringComparison.OrdinalIgnoreCase))
                return new StatusCodeResult(403);

            if (Directory.Exists(localPath))
                Directory.Delete(localPath, true);

            return new NoContentResult();
        }

        [HttpDelete("folder/{*targetPath}", Name = "DeleteFileSystemObjects")]
        public IActionResult DeleteFileSystemObjects([FromRoute] string targetPath, [FromBody] FileSystemInfoDto[] entries)
        {
            ValidateRequired(targetPath, nameof(targetPath));
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            targetPath = NormalizeTargetPath(targetPath);
            ValidateTargetPath(targetPath);
            ValidateRequired(entries, "entries");
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var localPath = _pathMap.ToLocalPath(targetPath);
            if (!Directory.Exists(localPath))
                return new NotFoundResult();

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
                        Errors = new string[1] { "'name' may not be null or empty string" }
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
            ValidateRequired(targetPath, nameof(targetPath));
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            targetPath = NormalizeTargetPath(targetPath);
            ValidateTargetPath(targetPath);
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var localPath = _pathMap.ToLocalPath(targetPath);
            if (!System.IO.File.Exists(localPath))
                return new NotFoundResult();

            var path = _pathMap.ToLocalPath(targetPath);
            return new FileStreamResult(System.IO.File.OpenRead(path), "text/plain");
        }

        [HttpPost("file/{*targetPath}", Name = "CreateFile")]
        public IActionResult Createfile([FromRoute] string targetPath, [FromForm] string name)
        {
            ValidateRequired(targetPath, nameof(targetPath));
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            targetPath = NormalizeTargetPath(targetPath);
            ValidateTargetPath(targetPath);
            ValidateRequired(name);
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var localPath = _pathMap.ToLocalPath(targetPath);
            if (!Directory.Exists(localPath))
                return new NotFoundResult();

            string fullPath = FilePath.Combine(localPath, name);
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
            ValidateRequired(targetPath, nameof(targetPath));
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            targetPath = NormalizeTargetPath(targetPath);
            ValidateTargetPath(targetPath);
            ValidateRequired(fsEntry, nameof(fsEntry));
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var filePath = _pathMap.ToLocalPath(targetPath);
            if (!System.IO.File.Exists(filePath))
                return new NotFoundResult();

            //Handle only content update now;
            using (StreamWriter writer = new StreamWriter(System.IO.File.OpenWrite(filePath)))
                await writer.WriteAsync(fsEntry.Content);

            return new NoContentResult();
        }

        [HttpDelete("file/{*targetPath}", Name = "DeleteFile")]
        public IActionResult DeleteFile([FromRoute] string targetPath)
        {
            ValidateRequired(targetPath, nameof(targetPath));
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            targetPath = NormalizeTargetPath(targetPath);
            ValidateTargetPath(targetPath);
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var filePath = _pathMap.ToLocalPath(targetPath);
            if (!System.IO.File.Exists(filePath))
                return new NotFoundResult();

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            return new NoContentResult();
        }
    }

}