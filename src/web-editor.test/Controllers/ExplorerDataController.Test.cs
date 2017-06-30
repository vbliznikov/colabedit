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
using Microsoft.AspNetCore.Mvc;

namespace CollabEdit.Test
{
    public class ExplorerDataControllerTest
    {
        private ILoggerFactory _logegrFactory;
        private ILogger<ExplorerDataController> _logger;
        private const string contentRoot = "../../../editor-root";
        private IOptions<ExplorerOptions> _options;

        public ExplorerDataControllerTest()
        {
            _logegrFactory = new LoggerFactory();
            _logegrFactory.AddConsole();
            _logger = _logegrFactory.CreateLogger<ExplorerDataController>();
            var mock = new Mock<IOptions<ExplorerOptions>>();
            mock.Setup(options => options.Value).Returns(new ExplorerOptions()
            {
                EditorRoot = Path.Combine(Directory.GetCurrentDirectory(), contentRoot)
            });
            _options = mock.Object;
        }
        [Fact]
        public void Test1()
        {
            _logger.LogInformation("Test started...");
            var controller = new ExplorerDataController(_logger, _options);

            ObjectResult result = Assert.IsAssignableFrom<ObjectResult>(controller.GetFolderContent("home"));
            Assert.NotNull(result?.Value);
            var enumerable = Assert.IsAssignableFrom<IEnumerable<FileSystemInfoDto>>(result.Value);
            Assert.Equal(0, enumerable.Count());

            _logger.LogInformation("Test end");
        }
    }
}