using log4net;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Songs.Core.Models.Exceptions;

namespace KaraW3B.Server.Songs.Core.Services.Scheduler
{
    public sealed class SchedulerService : ISchedulerService, IAsyncDisposable
    {
        private readonly ILog _logger = LogManager.GetLogger(nameof(SchedulerService));

        public async Task<ApiScheduler> RegisterSchedulerAsync(string schedulerName, int maxConcurrency, CancellationToken cancellationToken)
        {
            if (maxConcurrency < 1)
            {
                throw new KaraW3BSongsServerException("A scheduler must have at least 1 execution thread");
            }

            var schedulerBuilder = SchedulerBuilder.Create();
            schedulerBuilder.SchedulerName = schedulerName;
            schedulerBuilder.SchedulerId = Guid.NewGuid().ToString();
            var scheduler = await schedulerBuilder.UseDefaultThreadPool(x => x.MaxConcurrency = maxConcurrency).BuildScheduler();
            await scheduler.Start(cancellationToken);
            _logger.Info($"Scheduler '{scheduler.SchedulerName}' was started");
            return new ApiScheduler(scheduler, _logger);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var scheduler in SchedulerRepository.Instance.LookupAll())
            {
                _logger.Info(
                    $"Scheduler '{scheduler.SchedulerName}' is shutting down. Waiting for last running jobs to finish");
                await scheduler.Shutdown(true, CancellationToken.None);
                _logger.Info($"Scheduler '{scheduler.SchedulerName}' is now shutdown");
            }
        }
    }
}