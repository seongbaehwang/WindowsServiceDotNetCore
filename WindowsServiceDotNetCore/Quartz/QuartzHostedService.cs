using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;

namespace WindowsServiceDotNetCore.Quartz
{
    public class QuartzHostedService : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<JobSchedule> _jobSchedules;

        public QuartzHostedService(
            ISchedulerFactory schedulerFactory,
            IJobFactory jobFactory,
            IEnumerable<JobSchedule> jobSchedules)
        {
            _schedulerFactory = schedulerFactory;
            _jobSchedules = jobSchedules;
            _jobFactory = jobFactory;
        }
        public IScheduler Scheduler { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = _jobFactory;

            foreach (var jobSchedule in _jobSchedules)
            {
                var job = CreateJob(jobSchedule.JobType);
                var triggers = CreateTriggers(jobSchedule);

                await Scheduler.ScheduleJob(job, triggers, false, cancellationToken);
            }

            await Scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (Scheduler != null) await Scheduler.Shutdown(cancellationToken);
        }

        private static IJobDetail CreateJob(Type jobType)
        {
            return JobBuilder
                .Create(jobType)
                .WithIdentity(jobType.FullName)
                .WithDescription(jobType.Name)
                .Build();
        }

        private static IReadOnlyCollection<ITrigger> CreateTriggers(JobSchedule jobSchedule)
        {
            var schedules = jobSchedule.Schedules;
            var triggers = new List<ITrigger>(schedules.Count);

            var i = 0;
            foreach (var schedule in schedules)
            {
                i++;

                triggers.Add(TriggerBuilder
                    .Create()
                    .WithIdentity($"{jobSchedule.JobType.FullName}-{i}.trigger")
                    .WithCronSchedule(schedule)
                    .WithDescription(schedule)
                    .Build());
            }

            return triggers;
        }
    }
}