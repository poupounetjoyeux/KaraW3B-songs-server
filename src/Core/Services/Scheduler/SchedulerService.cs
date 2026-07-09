using System.Threading;
using System.Threading.Tasks;
using KaraW3B.SDK.Helpers;
using KaraW3B.Server.Core.Jobs;
using log4net;
using Quartz;

namespace KaraW3B.Server.Core.Services.Scheduler
{
    public sealed class SchedulerService : ISchedulerService
    {
        private readonly ILog _logger = LogManager.GetLogger(nameof(SchedulerService));

        private IScheduler _scheduler;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var schedulerBuilder = SchedulerBuilder.Create();
            schedulerBuilder.SchedulerName = $"{KaraW3BConstants.ApplicationName}_Scheduler";
            schedulerBuilder.SchedulerId = $"{KaraW3BConstants.ApplicationName}_Scheduler";
            _scheduler = await schedulerBuilder.UseDefaultThreadPool(x => x.MaxConcurrency = 5).BuildScheduler();
            await _scheduler.Start(cancellationToken);
            _logger.Info($"Scheduler '{_scheduler.SchedulerName}' was started");

            await RegisterJobs(cancellationToken);
        }

        private async Task RegisterJobs(CancellationToken cancellationToken)
        {
            var job = JobBuilder.Create<AnalyzeLibraryJob>()
                .WithIdentity(AnalyzeLibraryJob.JobKey)
                .StoreDurably()
                .Build();

            await _scheduler.AddJob(job, false, cancellationToken);
            _logger.Info($"Job {job.Key} was registered");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Info(
                $"Scheduler '{_scheduler.SchedulerName}' is shutting down. Waiting for last running jobs to finish");
            await _scheduler.Shutdown(true, cancellationToken);
            _logger.Info($"Scheduler '{_scheduler.SchedulerName}' is now shutdown");
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
    }
}