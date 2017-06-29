using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;

using CollabEdit.Controllers;
using CollabEdit.Model;

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
            var result = controller.GetFolderContent("home");

            _logger.LogInformation("Test end");
        }
    }
}