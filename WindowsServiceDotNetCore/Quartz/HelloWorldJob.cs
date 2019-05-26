using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace WindowsServiceDotNetCore.Quartz
{
    [DisallowConcurrentExecution]
    public class HelloWorldJob : IJob, IDisposable
    {
        private readonly ILogger<HelloWorldJob> _logger;
        public HelloWorldJob(ILogger<HelloWorldJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Hello world!");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // TODO: This is just a demo purpose
            _logger.LogDebug("Disposing");
        }
    }
}
