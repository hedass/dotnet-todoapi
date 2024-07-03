using Quartz.Impl;
using Quartz;
using Microsoft.Extensions.Hosting;

namespace onboarding.bll
{
    public class SchedulerService : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SchedulerService.StartAsync();
            await Task.Yield();
        }
        public static async Task StartAsync()
        {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();

            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<LoggingJob>()
                .WithIdentity("loggingJob", "group1")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("loggingTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(30)
                    .RepeatForever())
                .Build();

            // Schedule the job with the trigger
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
