using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Songs.Core.Models.Settings;
using KaraW3B.Server.Songs.Models.Helpers;

namespace KaraW3B.Server.Songs.Core.Services.Settings
{
    public sealed class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private readonly KaraW3BSettings _settings;

        public SettingsService(string settingsPath)
        {
            _settingsPath = settingsPath;
            _settings = LoadSettings();

            // This is in case some new options where added or the initial file not exist
            WriteSettings(CancellationToken.None).Wait();
        }

        public KaraW3BSettings LoadSettings()
        {
            if (!File.Exists(_settingsPath))
            {
                return new KaraW3BSettings();
            }

            using var settingsFile = new FileStream(_settingsPath, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<KaraW3BSettings>(settingsFile, JsonHelper.DefaultJsonSerializerOptions);

        }

        public event EventHandler SettingsUpdated;

        public KaraW3BSettings Settings => _settings.Clone();

        public async Task<bool> UpdateSettingsAsync(Action<KaraW3BSettings> updateSettings, CancellationToken cancellationToken)
        {
            if (updateSettings == null)
            {
                return false;
            }

            updateSettings(_settings);
            await WriteSettings(cancellationToken);
            SettingsUpdated?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private async Task WriteSettings(CancellationToken cancellationToken)
        {
            await using var settingsFile = new FileStream(_settingsPath, FileMode.OpenOrCreate, FileAccess.Write);
            await JsonSerializer.SerializeAsync(settingsFile, _settings, JsonHelper.DefaultJsonSerializerOptions, cancellationToken);
        }
    }
}
