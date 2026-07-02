using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace KaraW3B.Server.Core.Services.SchedulerService
{
    public interface ISchedulerService : IHostedService
    {
        Task StartJob(JobKey jobKey, JobDataMap dataMap, CancellationToken cancellationToken);
    }
}