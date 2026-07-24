using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using KaraW3B.Interpreters.Interfaces;
using KaraW3B.Server.Songs.Core.Persistence.Models.Libraries;
using KaraW3B.Server.Songs.Models.Songs;
using KaraW3B.Server.Songs.Models.Songs.Alerts;
using Microsoft.EntityFrameworkCore;

namespace KaraW3B.Server.Songs.Core.Persistence.Models.Songs
{
    [Table("Songs")]
    [PrimaryKey(nameof(Id))]
    public class DbSong : IAnalyzableSong
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Library))]
        public Guid LibraryId { get; set; }

        public virtual DbLibrary Library { get; set; }

        #region Core headers

        public Version Version { get; set; }

        public decimal Bpm { get; set; }

        [MaxLength(1000)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Artist { get; set; }

        public string Audio { get; set; }

        public TimeSpan? Gap { get; set; }

        public TimeSpan? Start { get; set; }

        public TimeSpan? End { get; set; }

        public virtual List<DbSongPlayer> Players { get; set; } = new();

        public Dictionary<int, string> GetPlayers()
        {
            return Players.ToDictionary(p => p.Number, p => p.Name);
        }

        #endregion

        #region Extra headers

        public string Cover { get; set; }

        public string Background { get; set; }

        public string Video { get; set; }

        public TimeSpan? VideoGap { get; set; }

        public string Vocals { get; set; }

        public string Instrumental { get; set; }

        public TimeSpan? PreviewStart { get; set; }

        public virtual DbSongMedley Medley { get; set; }

        public ISongMedley GetMedley()
        {
            return Medley;
        }

        public int? Year { get; set; }

        public List<string> Genres { get; set; } = new();

        public List<string> Languages { get; set; } = new();

        public List<string> Editions { get; set; } = new();

        public List<string> Tags { get; set; } = new();

        public List<string> Creators { get; set; } = new();

        public string ProvidedBy { get; set; }

        public string Comment { get; set; }

        public string AudioUrl { get; set; }

        public string VideoUrl { get; set; }

        public string CoverUrl { get; set; }

        public string BackgroundUrl { get; set; }

        [MaxLength(300)]
        public string Rendition { get; set; }

        public List<string> NotManagedHeaders { get; set; } = new();

        #endregion

        #region Internal

        public virtual List<DbSongAlert> Alerts { get; set; } = new();

        public virtual List<DbSongNote> Notes { get; set; } = new();

        [Required]
        public string SongFilePath { get; set; }

        [NotMapped]
        public string SongDirectory => Path.GetDirectoryName(SongFilePath);

        [Required]
        public string AnalyzedFileHash { get; set; }

        [Required]
        public DateTime LastParseTime { get; set; }

        [Required]
        public BrowserCompatibility AudioCompatibility { get; set; } = BrowserCompatibility.NotChecked;

        [Required]
        public BrowserCompatibility VideoCompatibility { get; set; } = BrowserCompatibility.NotChecked;

        [Required]
        public BrowserCompatibility VocalsCompatibility { get; set; } = BrowserCompatibility.NotChecked;

        [Required]
        public BrowserCompatibility InstrumentalCompatibility { get; set; } = BrowserCompatibility.NotChecked;

        #endregion

        public string GetSongFilePath(FileType fileType)
        {
            var filePath = fileType switch
            {
                FileType.Audio => Audio,
                FileType.Cover => Cover,
                FileType.Background => Background,
                FileType.Video => Video,
                _ => null
            };

            return string.IsNullOrEmpty(filePath) ? null : Path.Combine(SongDirectory, filePath);
        }

        public bool SongFileExist(FileType fileType)
        {
            var songFilePath = GetSongFilePath(fileType);
            return !string.IsNullOrEmpty(songFilePath) && File.Exists(songFilePath);
        }

        public bool IsSongFileCompatible(FileType fileType)
        {
            return fileType switch
            {
                FileType.Video => VideoCompatibility != BrowserCompatibility.ConversionMandatory,
                FileType.Audio => AudioCompatibility != BrowserCompatibility.ConversionMandatory,
                FileType.Instrumental => InstrumentalCompatibility != BrowserCompatibility.ConversionMandatory,
                FileType.Vocals => VocalsCompatibility != BrowserCompatibility.ConversionMandatory,
                _ => true
            };
        }

        public void SetBrowserCompatibilityStatus(FileType fileType, BrowserCompatibility browserCompatibility)
        {
            if (fileType is FileType.Cover or FileType.Background)
            {
                return;
            }

            switch (fileType)
            {
                case FileType.Audio:
                    AudioCompatibility = browserCompatibility;
                    break;
                case FileType.Video:
                    VideoCompatibility = browserCompatibility;
                    break;
                case FileType.Instrumental:
                    InstrumentalCompatibility = browserCompatibility;
                    break;
                case FileType.Vocals:
                    VocalsCompatibility = browserCompatibility;
                    break;
            }
        }

        public Song ToSong()
        {
            var songDto = new Song
            {
                Id = Id,
                Version = Version,
                Bpm = Bpm,
                Title = Title,
                Artist = Artist,
                Audio = Audio,
                Gap = Gap,
                Start = Start,
                End = End,
                Players = Players.Select(p => p.ToSongPlayer()).ToList(),
                Cover = Cover,
                Background = Background,
                Video = Video,
                Vocals = Vocals,
                Instrumental = Instrumental,
                AudioUrl = AudioUrl,
                VideoUrl = VideoUrl,
                CoverUrl = CoverUrl,
                BackgroundUrl = BackgroundUrl,
                VideoGap = VideoGap,
                PreviewStart = PreviewStart,
                Medley = Medley?.ToSongMedley(),
                Year = Year,
                Genres = Genres.ToList(),
                Languages = Languages.ToList(),
                Editions = Editions.ToList(),
                Tags = Tags.ToList(),
                Creators = Creators.ToList(),
                ProvidedBy = ProvidedBy,
                Comment = Comment,
                Rendition = Rendition,
                NotManagedHeaders = NotManagedHeaders.ToList(),
                LastParsedTime = LastParseTime,
                HasFatal = Alerts.Any(a => a.Level == AlertLevel.Fatal),
                HasErrors = Alerts.Any(a => a.Level == AlertLevel.Error),
                HasWarnings = Alerts.Any(a => a.Level == AlertLevel.Warning),
                AudioCompatibility = AudioCompatibility,
                VideoCompatibility = VideoCompatibility,
                VocalsCompatibility = VocalsCompatibility,
                InstrumentalCompatibility = InstrumentalCompatibility
            };
            return songDto;
        }

        public bool IsNotLoadable()
        {
            return VideoCompatibility == BrowserCompatibility.ConversionMandatory ||
                   AudioCompatibility == BrowserCompatibility.ConversionMandatory ||
                   InstrumentalCompatibility == BrowserCompatibility.ConversionMandatory ||
                   VocalsCompatibility == BrowserCompatibility.ConversionMandatory ||
                   Alerts.Any(a => a.Level == AlertLevel.Fatal);
        }
    }
}