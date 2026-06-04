namespace KaraWeb.Shared.Models.Songs.Notes
{
    /// <summary>
    ///     A song's note
    /// </summary>
    public sealed class SongNoteDto : IAnalyzableSongNote
    {
        /// <summary>
        ///     The note's type
        /// </summary>
        public NoteType Type { get; set; }

        /// <summary>
        ///     The related player
        /// </summary>
        public int PlayerNumber { get; set; }

        /// <summary>
        ///     The note's start beat
        /// </summary>
        public int StartBeat { get; set; }

        /// <summary>
        ///     The note's duration
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        ///     The note's pitch
        /// </summary>
        public int? Pitch { get; set; }

        /// <summary>
        ///     The note's text
        /// </summary>
        public string Text { get; set; }
    }
}