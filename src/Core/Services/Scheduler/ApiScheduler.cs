using System.Threading;
using System.Threading.Tasks;
using log4net;
using Quartz;

namespace KaraW3B.Server.Songs.Core.Services.Scheduler
{
    public sealed class ApiScheduler
    {
        private readonly IScheduler _scheduler;
        private readonly ILog _logger;

        public ApiScheduler(IScheduler scheduler, ILog logger)
        {
            _scheduler = scheduler;
            _logger = logger;
        }

        public async Task StartJob(JobKey jobKey, JobDataMap dataMap, CancellationToken cancellationToken)
        {
            if (!await _scheduler.CheckExists(jobKey, cancellationToken))
            {
                _logger.Error($"The job {jobKey} is not registered on the scheduler");
                return;
            }

            await _scheduler.TriggerJob(jobKey, dataMap, cancellationToken);
            _logger.Info($"The job {jobKey} was triggered");
        }

        public async Task RegisterJobAsync<TJob>(JobKey jobKey, CancellationToken cancellationToken) where TJob : IJob
        {
            var job = JobBuilder.Create<TJob>()
                .WithIdentity(jobKey)
                .StoreDurably()
                .Build();

            await _scheduler.AddJob(job, false, cancellationToken);
            _logger.Info($"Job {job.Key} was registered");
        }
    }
}
