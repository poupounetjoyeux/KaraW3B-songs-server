using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.SDK.Helpers;
using KaraW3B.Server.Core.Models.Settings;

namespace KaraW3B.Server.Core.Services.Settings
{
    public sealed class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private KaraW3BSettings _settings;

        public SettingsService(string settingsPath)
        {
            _settingsPath = settingsPath;
        }

        public async Task<KaraW3BSettings> GetSettingsAsync(CancellationToken cancellationToken)
        {
            if (_settings != null)
            {
                return _settings;
            }

            if (!File.Exists(_settingsPath))
            {
                _settings = new KaraW3BSettings();
                await WriteSettings(_settings, cancellationToken);
                return _settings;
            }

            await using var settingsFile = new FileStream(_settingsPath, FileMode.Open, FileAccess.Read);
            _settings = await JsonSerializer.DeserializeAsync<KaraW3BSettings>(settingsFile, JsonHelper.DefaultJsonSerializerOptions, cancellationToken);
            return _settings;
        }

        public async Task<bool> UpdateSettingsAsync(Action<KaraW3BSettings> updateSettings, CancellationToken cancellationToken)
        {
            if (updateSettings == null)
            {
                return false;
            }

            var settings = await GetSettingsAsync(cancellationToken);
            updateSettings(settings);
            await WriteSettings(settings, cancellationToken);
            return true;
        }

        private async Task WriteSettings(KaraW3BSettings settings, CancellationToken cancellationToken)
        {
            await using var settingsFile = new FileStream(_settingsPath, FileMode.OpenOrCreate, FileAccess.Write);
            await JsonSerializer.SerializeAsync(settingsFile, settings, JsonHelper.DefaultJsonSerializerOptions, cancellationToken);
        }
    }
}
