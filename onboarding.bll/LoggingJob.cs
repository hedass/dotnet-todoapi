using Quartz;

namespace onboarding.bll
{
    public class LoggingJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Logging something at: " + DateTime.Now);

            return Task.CompletedTask;
        }
    }
}
