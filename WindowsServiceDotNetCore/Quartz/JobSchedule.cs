using System;
using System.Collections.Generic;

namespace WindowsServiceDotNetCore.Quartz
{
    public class JobSchedule
    {
        /// <summary>
        /// Create an instance of <see cref="JobSchedule"/>
        /// </summary>
        /// <param name="jobType">Type of job to schedule</param>
        /// <param name="schedules">List of CRON expression schedules</param>
        public JobSchedule(Type jobType, IReadOnlyCollection<string> schedules)
        {
            JobType = jobType;
            Schedules = schedules;
        }

        public Type JobType { get; }

        /// <summary>
        /// List of CRON expression schedules
        /// </summary>
        public IReadOnlyCollection<string> Schedules { get; }
    }
}