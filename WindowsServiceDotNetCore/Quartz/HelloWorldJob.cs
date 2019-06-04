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
        private readonly ScopedObject _scopedObject;

        public HelloWorldJob(ILogger<HelloWorldJob> logger, ScopedObject scopedObject)
        {
            _logger = logger;
            _scopedObject = scopedObject;
            _logger.LogDebug($"***** {nameof(HelloWorldJob)} is being created *****");
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Hello world!");
            _scopedObject.Hello();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogDebug($"***** {nameof(HelloWorldJob)} is disposing *****");
        }
    }

    public class ScopedObject:IDisposable
    {
        private readonly ILogger<ScopedObject> _logger;

        public ScopedObject(ILogger<ScopedObject> logger)
        {
            _logger = logger;
            _logger.LogDebug($"***** {nameof(ScopedObject)} is being created *****");
        }

        public void Hello()
        {
            _logger.LogInformation($"Hello from {nameof(ScopedObject)}");
        }

        public void Dispose()
        {
            _logger.LogDebug($"***** {nameof(ScopedObject)} is disposing *****");
        }
    }
}
