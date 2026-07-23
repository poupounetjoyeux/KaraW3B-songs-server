using System;
using System;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Songs.Core.Models.Settings;

namespace KaraW3B.Server.Songs.Core.Services.Settings
{
    public interface ISettingsService
    {
        event EventHandler SettingsUpdated;
        KaraW3BSettings Settings { get; }
        Task<bool> UpdateSettingsAsync(Action<KaraW3BSettings> updateSettings, CancellationToken cancellationToken);
    }
}
