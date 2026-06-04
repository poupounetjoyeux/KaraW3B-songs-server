using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using KaraWeb.Core.Persistence.Libraries;
using KaraWeb.Shared.Models.Songs;

namespace KaraWeb.Core.Persistence.Songs
{
    [Table("Songs")]
    public sealed class Song : IAnalyzableSong
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Library))]
        public Guid LibraryId { get; set; }

        #region Core headers

        [MaxLength(6)]
        public string Version { get; set; }

        public double? Bpm { get; set; }

        [MaxLength(1000)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Artist { get; set; }

        public string Audio { get; set; }

        public int? Gap { get; set; }

        public int? Start { get; set; }

        public int? End { get; set; }

        public List<SongPlayer> Players { get; set; } = new();

        public Dictionary<int, string> GetPlayers()
        {
            return Players.ToDictionary(p => p.Number, p => p.Name);
        }

        #endregion

        #region Extra headers

        public string Cover { get; set; }

        public string Background { get; set; }

        public string Video { get; set; }

        public int? VideoGap { get; set; }

        public string Vocals { get; set; }

        public string Instrumental { get; set; }

        public int? PreviewStart { get; set; }

        public int? MedleyStart { get; set; }

        public int? MedleyEnd { get; set; }

        public int? Year { get; set; }

        public List<string> Genres { get; set; } = new();

        public List<string> Languages { get; set; } = new();

        public List<string> Editions { get; set; } = new();

        public List<string> Tags { get; set; } = new();

        [MaxLength(500)]
        public string Creator { get; set; }

        public string ProvidedBy { get; set; }

        public string Comment { get; set; }

        public string AudioUrl { get; set; }

        public string VideoUrl { get; set; }

        public string CoverUrl { get; set; }

        public string BackgroundUrl { get; set; }

        [MaxLength(300)]
        public string Rendition { get; set; }

        [MaxLength(25)]
        public string Encoding { get; set; }

        public List<string> NotManagedHeaders { get; set; } = new();

        #endregion

        #region Internal

        public List<string> Errors { get; set; } = new();

        public List<string> Warnings { get; set; } = new();

        public List<SongNote> Notes { get; set; } = new();

        public string SongFilePath { get; set; }

        [Required]
        public string AnalyzedFileHash { get; set; }

        #endregion

        private void FeedBaseSongDto(SongDtoBase songDto)
        {
            songDto.Id = Id;
            songDto.Version = Version;
            songDto.Bpm = Bpm;
            songDto.Title = Title;
            songDto.Artist = Artist;
            songDto.Gap = Gap;
            songDto.Start = Start;
            songDto.End = End;
            songDto.Players = Players.ToDictionary(p => p.Number, p => p.Name);
            songDto.VideoGap = VideoGap;
            songDto.PreviewStart = PreviewStart;
            songDto.MedleyStart = MedleyStart;
            songDto.MedleyEnd = MedleyEnd;
            songDto.Year = Year;
            songDto.Genres = Genres.ToList();
            songDto.Languages = Languages.ToList();
            songDto.Editions = Editions.ToList();
            songDto.Tags = Tags.ToList();
            songDto.Creator = Creator;
            songDto.ProvidedBy = ProvidedBy;
            songDto.Comment = Comment;
            songDto.Rendition = Rendition;
            songDto.Encoding = Encoding;
        }

        public SongDto ToDto()
        {
            var songDto = new SongDto
            {
                HasAudio = !string.IsNullOrEmpty(Audio),
                HasCover = !string.IsNullOrEmpty(Cover),
                HasBackground = !string.IsNullOrEmpty(Background),
                HasVideo = !string.IsNullOrEmpty(Video),
                HasVocals = !string.IsNullOrEmpty(Vocals),
                HasInstrumental = !string.IsNullOrEmpty(Instrumental),
                HasErrors = Errors.Count > 0,
                HasWarnings = Warnings.Count > 0
            };
            FeedBaseSongDto(songDto);
            return songDto;
        }

        public DetailedSongDto ToDetailedDto()
        {
            var detailedSongDto = new DetailedSongDto
            {
                Audio = Audio,
                Cover = Cover,
                Background = Background,
                Video = Video,
                Vocals = Vocals,
                Instrumental = Instrumental,
                AudioUrl = AudioUrl,
                VideoUrl = VideoUrl,
                CoverUrl = CoverUrl,
                BackgroundUrl = BackgroundUrl,
                NotManagedHeaders = NotManagedHeaders.ToList(),
                Errors = Errors.ToList(),
                Warnings = Warnings.ToList(),
                Notes = Notes.Select(n => n.ToDto()).ToList()
            };
            FeedBaseSongDto(detailedSongDto);
            return detailedSongDto;
        }
    }
}