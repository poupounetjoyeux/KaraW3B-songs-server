namespace KaraW3B.Server.Core.Models.Settings
{
    public sealed class ConversionSettings
    {
        /// <summary>
        /// Flag to disable/enable the conversion
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// A threshold to know what we must convert
        /// </summary>
        public ConversionStatus Threshold { get; set; } = ConversionStatus.Mandatory;

        /// <summary>
        /// A flag to enable/disable conversion directly after library analyze
        /// </summary>
        public bool AfterLibraryAnalyze { get; set; } = true;

        /// <summary>
        /// A CRON to run conversion jobs regularly
        /// </summary>
        public string Cron { get; set; }
    }
}
