using System;

namespace KaraW3B.Server.Songs.Core.Models.Settings
{
    public sealed class KaraW3BSettings
    {
        /// <summary>
        /// A flag to disable/enable Swagger GEN & Swagger UI
        /// </summary>
        public bool SwaggerEnabled { get; set; } = true;

        /// <summary>
        /// A custom path to FFmpeg binaries
        /// If not specified, will try to get it from PATH
        /// </summary>
        public string FFmpegPath { get; set; }

        /// <summary>
        /// Settings related to threads limits
        /// </summary>
        public ConcurrencySettings ConcurrencySettings { get; set; } = new();

        /// <summary>
        /// The file format version used to write song files
        /// By default will use unversioned one
        /// </summary>
        public Version FileVersionToWrite { get; set; } = null;

        public KaraW3BSettings Clone()
        {
            return new KaraW3BSettings
            {
                FFmpegPath = FFmpegPath,
                SwaggerEnabled = SwaggerEnabled,
                ConcurrencySettings = ConcurrencySettings?.Clone() ?? new ConcurrencySettings()
            };
        }
    }
}
