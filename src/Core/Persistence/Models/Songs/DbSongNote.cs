using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KaraW3B.Interpreters.Interfaces;
using KaraW3B.Server.Songs.Models.Songs.Notes;
using Microsoft.EntityFrameworkCore;

namespace KaraW3B.Server.Songs.Core.Persistence.Models.Songs
{
    [Table("SongNotes")]
    [PrimaryKey(nameof(SongId), nameof(FileLine))]
    [Index(nameof(PlayerNumber))]
    public class DbSongNote : ISongNote
    {
        public int FileLine { get; set; }

        [NotMapped]
        public char Type => GetNoteType();

        public Guid SongId { get; set; }

        [ForeignKey(nameof(SongId))]
        public virtual DbSong Song { get; set; }

        [Required]
        public NoteType NoteType { get; set; }

        [Required]
        public int PlayerNumber { get; set; }

        [Required]
        public int StartBeat { get; set; }

        public int? Duration { get; set; }

        public int? Pitch { get; set; }

        public string Text { get; set; }

        public SongNote ToSongNote()
        {
            return new SongNote
            {
                FileLine = FileLine,
                NoteType = NoteType,
                PlayerNumber = PlayerNumber,
                StartBeat = StartBeat,
                Duration = Duration,
                Pitch = Pitch,
                Text = Text
            };
        }

        public static NoteType ParseNoteType(char noteType)
        {
            return char.ToUpperInvariant(noteType) switch
            {
                ':' => NoteType.Regular,
                '*' => NoteType.Golden,
                'R' => NoteType.Rap,
                'G' => NoteType.GoldenRap,
                '-' => NoteType.EndOfPhrase,
                _ => NoteType.Freestyle
            };
        }

        public char GetNoteType()
        {
            return NoteType switch
            {
                NoteType.Regular => ':',
                NoteType.Golden => '*',
                NoteType.Rap => 'R',
                NoteType.GoldenRap => 'G',
                NoteType.EndOfPhrase => '-',
                _ => 'F'
            };
        }
    }
}