using System.Threading;
using System.Threading.Tasks;

namespace KaraW3B.Server.Songs.Core.Services.Scheduler
{
    public interface ISchedulerService
    {
        Task<ApiScheduler> RegisterSchedulerAsync(string schedulerName, int maxConcurrency, CancellationToken cancellationToken);
    }
}