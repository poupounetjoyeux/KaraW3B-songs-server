using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KaraW3B.SDK.Models.Songs.Notes;
using Microsoft.EntityFrameworkCore;

namespace KaraW3B.Server.Core.Persistence.Models.Songs
{
    [Table("SongNotes")]
    [PrimaryKey(nameof(SongId), nameof(FileLine))]
    [Index(nameof(PlayerNumber))]
    public class SongNote : IAnalyzableSongNote
    {
        public int FileLine { get; set; }

        public Guid SongId { get; set; }

        [ForeignKey(nameof(SongId))]
        public virtual Song Song { get; set; }

        [Required]
        public NoteType Type { get; set; }

        [Required]
        public int PlayerNumber { get; set; }

        [Required]
        public int StartBeat { get; set; }

        public int? Duration { get; set; }

        public int? Pitch { get; set; }

        public string Text { get; set; }

        public SongNoteDto ToDto()
        {
            return new SongNoteDto
            {
                FileLine = FileLine,
                Type = Type,
                PlayerNumber = PlayerNumber,
                StartBeat = StartBeat,
                Duration = Duration,
                Pitch = Pitch,
                Text = Text
            };
        }
    }
}