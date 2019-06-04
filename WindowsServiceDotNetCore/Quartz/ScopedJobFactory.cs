using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;

namespace WindowsServiceDotNetCore.Quartz
{
    /// <summary>
    /// https://github.com/AndyPook/QuartzHostedService
    /// </summary>
    public class ScopedJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScopedJobFactory> _logger;

        public ScopedJobFactory(IServiceProvider serviceProvider, ILogger<ScopedJobFactory> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            _logger.LogDebug($"***** {nameof(NewJob)} *****");

            var jobType = bundle.JobDetail.JobType;

            // MA - Generate a scope for the job, this allows the job to be registered
            //	using .AddScoped<T>() which means we can use scoped dependencies 
            //	e.g. database contexts
            var scope = _serviceProvider.CreateScope();

            var job = (IJob)scope.ServiceProvider.GetRequiredService(jobType);

            return new ScopedJob(scope, job);
        }

        public void ReturnJob(IJob job)
        {
            _logger.LogDebug($"***** {nameof(ReturnJob)} *****");

            (job as IDisposable)?.Dispose();
        }
        
        private class ScopedJob : IJob, IDisposable
        {
            private readonly IServiceScope _scope;
            private readonly IJob _innerJob;

            public ScopedJob(IServiceScope scope, IJob innerJob)
            {
                _scope = scope;
                _innerJob = innerJob;
            }

            public Task Execute(IJobExecutionContext context) => _innerJob.Execute(context);
            
            public void Dispose()
            {
                _scope.Dispose();
            }
        }
    }
}