using System;
using KaraW3B.Server.Core.Models.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace KaraW3B.Server.Core.Services.Settings
{
    public interface ISettingsService
    {
        Task<KaraW3BSettings> GetSettingsAsync(CancellationToken cancellationToken);
        Task<bool> UpdateSettingsAsync(Action<KaraW3BSettings> updateSettings, CancellationToken cancellationToken);
    }
}
