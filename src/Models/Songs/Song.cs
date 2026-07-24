using System;
using System.Collections.Generic;
using System.Linq;

namespace KaraW3B.Server.Songs.Models.Songs
{
    /// <summary>
    ///     Base properties of a song parsed from file
    /// </summary>
    public sealed class Song
    {
        /// <summary>
        ///     The song's ID
        /// </summary>
        public Guid Id { get; set; }

        #region Core headers

        /// <summary>
        ///     The UltraStar format version used
        /// </summary>

        public Version Version { get; set; }

        /// <summary>
        ///     The BPM of the song
        /// </summary>
        public decimal Bpm { get; set; }

        /// <summary>
        ///     The song's title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     The song's artist
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        ///     The audio file path
        /// </summary>
        /// <remarks>must be a relative path</remarks>
        public string Audio { get; set; }

        /// <summary>
        ///     The GAP between start of the audio and first beat
        /// </summary>
        public TimeSpan? Gap { get; set; }

        /// <summary>
        ///     The time relative to the start of the audio file when the song should start
        /// </summary>
        public TimeSpan? Start { get; set; }

        /// <summary>
        ///     The time relative to the start of the audio file when the song should stop
        /// </summary>
        public TimeSpan? End { get; set; }

        /// <summary>
        ///     Song's players defined from #P1 to #P9
        /// </summary>
        public List<SongPlayer> Players { get; set; } = new();

        public Dictionary<int, string> GetPlayers()
        {
            return Players.ToDictionary(p => p.PlayerNumber, p => p.Name);
        }

        #endregion

        #region Extra headers

        /// <summary>
        ///     The cover file path
        /// </summary>
        /// <remarks>must be a relative path</remarks>
        public string Cover { get; set; }

        /// <summary>
        ///     The background file path
        /// </summary>
        /// <remarks>must be a relative path</remarks>
        public string Background { get; set; }

        /// <summary>
        ///     The video file path
        /// </summary>
        /// <remarks>must be a relative path</remarks>
        public string Video { get; set; }

        /// <summary>
        ///     The vocals file path
        /// </summary>
        /// <remarks>must be a relative path</remarks>
        public string Vocals { get; set; }

        /// <summary>
        ///     The instrumental file path
        /// </summary>
        /// <remarks>must be a relative path</remarks>
        public string Instrumental { get; set; }

        /// <summary>
        ///     The source of the audio used
        /// </summary>
        /// <remarks>valid http(s) URL according to RFC 1738</remarks>
        public string AudioUrl { get; set; }

        /// <summary>
        ///     The source of the video used
        /// </summary>
        /// <remarks>valid http(s) URL according to RFC 1738</remarks>
        public string VideoUrl { get; set; }

        /// <summary>
        ///     The source of the cover used
        /// </summary>
        /// <remarks>valid http(s) URL according to RFC 1738</remarks>
        public string CoverUrl { get; set; }

        /// <summary>
        ///     The source of the background used
        /// </summary>
        /// <remarks>valid http(s) URL according to RFC 1738</remarks>
        public string BackgroundUrl { get; set; }

        /// <summary>
        ///     A set of headers not managed by the application
        /// </summary>
        public List<string> NotManagedHeaders { get; set; } = new();

        /// <summary>
        ///     The video delay relative to the audio file
        /// </summary>
        public TimeSpan? VideoGap { get; set; }

        /// <summary>
        ///     The offset of audio file to use for preview
        /// </summary>
        public TimeSpan? PreviewStart { get; set; }

        /// <summary>
        ///     The song medley information
        /// </summary>
        public SongMedley Medley { get; set; }

        /// <summary>
        ///     The song's year
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        ///     Song's genres
        /// </summary>
        public List<string> Genres { get; set; } = new();

        /// <summary>
        ///     The song's languages
        /// </summary>
        /// <remarks>Using ISO 639-2 english languages</remarks>
        public List<string> Languages { get; set; } = new();

        /// <summary>
        ///     Song's editions
        /// </summary>
        public List<string> Editions { get; set; } = new();

        /// <summary>
        ///     Song's tags
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        ///     The song's creators
        /// </summary>
        public List<string> Creators { get; set; } = new();

        /// <summary>
        ///     The source of the song
        /// </summary>
        /// <remarks>valid http(s) URL according to RFC 1738</remarks>
        public string ProvidedBy { get; set; }

        /// <summary>
        ///     Comment about the song
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        ///     Specific recording of the song
        /// </summary>
        public string Rendition { get; set; }

        #endregion

        #region Internal

        /// <summary>
        ///     A flag that indicates if the song contains error alerts
        /// </summary>
        public bool HasFatal { get; set; }

        /// <summary>
        ///     A flag that indicates if the song contains error alerts
        /// </summary>
        public bool HasErrors { get; set; }

        /// <summary>
        ///     A flag that indicates if the song contains warning alerts
        /// </summary>
        public bool HasWarnings { get; set; }

        /// <summary>
        ///     The last time when the song was parsed from disk's file
        /// </summary>
        public DateTime LastParsedTime { get; set; }

        /// <summary>
        ///     The audio file compatibility status with web browsers
        /// </summary>
        public BrowserCompatibility AudioCompatibility { get; set; }

        /// <summary>
        ///     The video file compatibility status with web browsers
        /// </summary>
        public BrowserCompatibility VideoCompatibility { get; set; }

        /// <summary>
        ///     The vocals file compatibility status with web browsers
        /// </summary>
        public BrowserCompatibility VocalsCompatibility { get; set; }

        /// <summary>
        ///     The instrumental file compatibility status with web browsers
        /// </summary>
        public BrowserCompatibility InstrumentalCompatibility { get; set; }

        #endregion
    }
}
