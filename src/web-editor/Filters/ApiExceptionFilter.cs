using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CollabEdit.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute, IFilterFactory
    {
        public ApiExceptionFilterAttribute() { }

        bool IFilterFactory.IsReusable => true;
        IFilterMetadata IFilterFactory.CreateInstance(IServiceProvider serviceProvider)
        {
            var hostingEnvironment = (IHostingEnvironment)serviceProvider.GetService(typeof(IHostingEnvironment));
            var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
            var logger = loggerFactory.CreateLogger<ApiExceptionFilter>();
            return new ApiExceptionFilter(hostingEnvironment, logger);
        }
    }

    public class ApiExceptionFilter : IExceptionFilter
    {
        private IHostingEnvironment _hostingEnvironment;
        private ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(IHostingEnvironment hostingEnvironment, ILogger<ApiExceptionFilter> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError("Unhandled exception: {0}", context.Exception);
            _logger.LogInformation("Set API exception descrption object to Result.");
            Object exceptionData;
            if (_hostingEnvironment.IsDevelopment())
            {
                exceptionData = new
                {
                    apiErrors = new string[] { context.Exception.Message },
                    stackTrace = context.Exception.StackTrace
                };
            }
            else
            {
                exceptionData = new
                {
                    apiErrors = new string[] { (string)context.Exception.Message }
                };
            }

            var result = new ObjectResult(exceptionData);
            result.StatusCode = 500;
            context.Result = result;
        }
    }
}