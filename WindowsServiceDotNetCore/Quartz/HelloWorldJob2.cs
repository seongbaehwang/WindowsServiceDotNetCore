using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace WindowsServiceDotNetCore.Quartz
{
    [DisallowConcurrentExecution]
    public class HelloWorldJob2 : IJob
    {
        private readonly ILogger<HelloWorldJob> _logger;
        public HelloWorldJob2(ILogger<HelloWorldJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Hello from {nameof(HelloWorldJob2)}");
            return Task.CompletedTask;
        }
    }
}