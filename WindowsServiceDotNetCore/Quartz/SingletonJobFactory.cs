using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace WindowsServiceDotNetCore.Quartz
{
    public class SingletonJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SingletonJobFactory> _logger;

        public SingletonJobFactory(IServiceProvider serviceProvider, ILogger<SingletonJobFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            _logger.LogDebug("Cleaning process has started.");

            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}