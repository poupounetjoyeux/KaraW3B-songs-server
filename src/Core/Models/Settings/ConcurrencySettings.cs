namespace KaraW3B.Server.Songs.Core.Models.Settings
{
    public sealed class ConcurrencySettings
    {
        /// <summary>
        /// The maximum song file parsed in parallel
        /// This value is per library analyze
        /// </summary>
        public int MaxSongParsingConcurrency { get; set; } = 10;

        /// <summary>
        /// The maximum libraries that can be analyzed in parallel
        /// </summary>
        public int MaxLibraryAnalyzesConcurrency { get; set; } = 2;

        public ConcurrencySettings Clone()
        {
            return new ConcurrencySettings
            {
                MaxLibraryAnalyzesConcurrency = MaxLibraryAnalyzesConcurrency,
                MaxSongParsingConcurrency = MaxSongParsingConcurrency
            };
        }
    }
}
