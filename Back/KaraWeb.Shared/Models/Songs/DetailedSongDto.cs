using System.Collections.Generic;
using KaraWeb.Shared.Models.Songs.Notes;

namespace KaraWeb.Shared.Models.Songs
{
    /// <summary>
    ///     A song parsed from file with all its details
    /// </summary>
    public sealed class DetailedSongDto : SongDtoBase, IAnalyzableSong
    {
        #region Core headers

        /// <summary>
        ///     The audio file path
        /// </summary>
        /// <remarks>must be a relative path</remarks>
        public string Audio { get; set; }

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

        #endregion

        #region Internal

        /// <summary>
        ///     The set of all song's notes
        /// </summary>
        public List<SongNoteDto> Notes { get; set; }

        /// <summary>
        ///     The set of errors the song file produces
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        ///     The set of warnings the song file produces
        /// </summary>
        public List<string> Warnings { get; set; }

        #endregion
    }
}