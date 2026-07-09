namespace KaraW3B.Server.Core.Models.Settings
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
        /// Settings related to the conversion of video files
        /// </summary>
        public ConversionSettings VideoConversion { get; set; } = new();

        /// <summary>
        /// Settings related to the conversion of audio files
        /// </summary>
        public ConversionSettings AudioConversion { get; set; } = new();
    }
}
