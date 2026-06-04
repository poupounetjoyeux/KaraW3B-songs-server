using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KaraWeb.Shared.Models.Songs.Notes;

namespace KaraWeb.Core.Persistence.Songs
{
    [Table("SongNotes")]
    public sealed class SongNote : IAnalyzableSongNote
    {
        [Key]
        public Guid NoteId { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Song))]
        public Guid SongId { get; set; }

        public NoteType Type { get; set; }

        public int PlayerNumber { get; set; }

        public int StartBeat { get; set; }

        public int? Duration { get; set; }

        public int? Pitch { get; set; }

        public string Text { get; set; }

        public SongNoteDto ToDto()
        {
            return new SongNoteDto
            {
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