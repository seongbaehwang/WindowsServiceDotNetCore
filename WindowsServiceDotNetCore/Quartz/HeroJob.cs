using System;
using System.Threading.Tasks;
using WindowsServiceDotNetCore.Entities;
using Microsoft.Extensions.Logging;
using Quartz;

namespace WindowsServiceDotNetCore.Quartz
{
    [DisallowConcurrentExecution]
    public class HeroJob : IJob
    {
        private readonly ILogger<HeroJob> _logger;
        private readonly HeroContext _heroContext;

        public HeroJob(ILogger<HeroJob> logger, HeroContext heroContext)
        {
            _logger = logger;
            _heroContext = heroContext;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("A hero was added");
            _heroContext.Heroes.Add(new Hero { Name = $"Superman -{DateTime.Now}" });
            await _heroContext.SaveChangesAsync();
        }
    }
}