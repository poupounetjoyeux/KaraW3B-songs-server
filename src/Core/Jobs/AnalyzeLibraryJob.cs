using KaraW3B.Server.Songs.Core.Helpers;
using KaraW3B.Server.Songs.Core.Models;
using KaraW3B.Server.Songs.Core.Persistence;
using KaraW3B.Server.Songs.Core.Persistence.Models.Libraries;
using KaraW3B.Server.Songs.Core.Persistence.Models.Songs;
using KaraW3B.Server.Songs.Core.Services.FFmpeg;
using KaraW3B.Server.Songs.Core.Services.SongFileInterpreter;
using KaraW3B.Server.Songs.Models.Libraries;
using KaraW3B.Server.Songs.Models.Songs;
using KaraW3B.Server.Songs.Models.Songs.Alerts;
using log4net;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace KaraW3B.Server.Songs.Core.Jobs
{
    public sealed class AnalyzeLibraryJob : IJob
    {
        public static readonly JobKey JobKey = new(nameof(AnalyzeLibraryJob), KaraW3BConstants.ApplicationName);
        private readonly ILog _logger = LogManager.GetLogger(JobKey.Name);

        public const string LibraryIdKey = "library_id";
        public const string LibraryPathKey = "library_path";
        public const string AnalyzeTypeKey = "analyze_type";
        public const string SongParserServiceKey = "song_parser_service";
        public const string MaxParallelismKey = "max_parallelism";
        public const string FFmpegServiceKey = "FFmpeg_service";

        public async Task Execute(IJobExecutionContext context)
        {
            if (context.MergedJobDataMap[LibraryIdKey] is not Guid libraryId)
            {
                _logger.Error("Unable to retrieve a valid library ID from job context");
                return;
            }

            if (context.MergedJobDataMap[LibraryPathKey] is not string libraryPath || string.IsNullOrEmpty(libraryPath))
            {
                _logger.Error("Unable to retrieve a valid library path from job context");
                return;
            }

            if (context.MergedJobDataMap[AnalyzeTypeKey] is not LibraryAnalyzeType analyzeType)
            {
                _logger.Error("Unable to retrieve a valid analyze type from job context");
                return;
            }

            if (context.MergedJobDataMap[SongParserServiceKey] is not ISongFileInterpreterService songParserService)
            {
                _logger.Error("Unable to retrieve a valid song parser service from job context");
                return;
            }

            if (context.MergedJobDataMap[MaxParallelismKey] is not int maxParallelism)
            {
                _logger.Error("Unable to retrieve a valid max parallelism value from job context");
                return;
            }

            if (context.MergedJobDataMap[FFmpegServiceKey] is not IFFmpegService ffmpegService)
            {
                _logger.Error("Unable to retrieve a valid FFmpeg service from job context");
                return;
            }

            await using var dbContext = new KaraW3BDbContext();
            if (!await DbLibrary.TryMarkAsAnalyzingAsync(dbContext, libraryId, context.CancellationToken))
            {
                _logger.Warn($"Library with ID {libraryId} is not currently queued for analyze.. Aborting..");
                return;
            }
            

            var directory = new DirectoryInfo(libraryPath);
            if (!directory.Exists)
            {
                var message = $"Directory '{directory.FullName}' doesn't exist";
                await DbLibrary.MarkAs(dbContext, libraryId, false, message, context.CancellationToken);
                _logger.Error(message);
                return;
            }

            _logger.Info($"Start analyzing library '{libraryId}' in mode '{analyzeType}'");
            var timeWatcher = new Stopwatch();
            timeWatcher.Start();

            try
            {
                var foundFiles = directory.GetFiles("*.txt", SearchOption.AllDirectories).OrderBy(f => f.Name).ToArray();
                _logger.Info($"Found {foundFiles.Length} potential song file(s) to analyze");

                var parsedSongIds = new ConcurrentBag<Guid>();
                await Parallel.ForEachAsync(foundFiles,
                    new ParallelOptions
                        { CancellationToken = context.CancellationToken, MaxDegreeOfParallelism = maxParallelism },
                    (f, c) => ProcessSongFile(songParserService, ffmpegService, libraryId, analyzeType, parsedSongIds,
                        f, c));

                var songsToDelete =
                     await dbContext.Songs.Where(s => s.LibraryId == libraryId && !parsedSongIds.Contains(s.Id))
                        .ToListAsync(context.CancellationToken);
                if (songsToDelete.Count > 0)
                {
                    dbContext.RemoveRange(songsToDelete);
                    await dbContext.SaveChangesAsync(context.CancellationToken);
                }

                timeWatcher.Stop();

                var message = $"Library '{libraryId}' analyzed {parsedSongIds.Count} song(s) and deleted {songsToDelete.Count} song(s) in {timeWatcher.Elapsed} ms";
                await DbLibrary.MarkAs(dbContext, libraryId, true, message, context.CancellationToken);
                _logger.Info(message);
            }
            catch(Exception e) 
            {
                timeWatcher.Stop();
                var message = $"The library analyze encounter an exception after {timeWatcher.Elapsed} ms: {e}";
                await DbLibrary.MarkAs(dbContext, libraryId, false, message, context.CancellationToken);
                _logger.Error(message);
            }
        }

        private static async Task<string> ComputeFileHash(FileInfo file, CancellationToken cancellationToken)
        {
            var fileBytes = await File.ReadAllBytesAsync(file.FullName, cancellationToken);
            var hashBytes = SHA1.HashData(fileBytes);
            return Convert.ToHexStringLower(hashBytes);
        }

        private async ValueTask ProcessSongFile(ISongFileInterpreterService songFileInterpreterService, IFFmpegService fFmpegService, Guid libraryId,
            LibraryAnalyzeType analyzeType,
            ConcurrentBag<Guid> parsedSongIds, FileInfo songFile, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Info($"Starting analyze of song file '{songFile.FullName}'");
                await using var dbContext = new KaraW3BDbContext();
                var song = await dbContext.Songs
                    .SingleOrDefaultAsync(
                        s =>
                            s.SongFilePath == songFile.FullName &&
                            s.LibraryId == libraryId, cancellationToken);

                var fileHash = await ComputeFileHash(songFile, cancellationToken);

                var needParsing = true;
                if (analyzeType == LibraryAnalyzeType.Optimized)
                {
                    if (song != null && song.AnalyzedFileHash == fileHash)
                    {
                        _logger.Info($"Same hash already in DB for file '{songFile.FullName}'");
                        needParsing = false;
                    }
                }

                if (needParsing)
                {
                    var isNew = false;
                    if (song == null)
                    {
                        song = new DbSong
                        {
                            LibraryId = libraryId,
                            SongFilePath = songFile.FullName,
                            AnalyzedFileHash = fileHash
                        };
                        isNew = true;
                    }

                    if (!await songFileInterpreterService.ParseSongAndCheckAsync(songFile, song, cancellationToken))
                    {
                        _logger.Error($"Unable to parse data from '{songFile.FullName}'.. Ignoring it..");
                        return;
                    }

                    if (isNew)
                    {
                        await dbContext.Songs.AddAsync(song, cancellationToken);
                    }
                }
                else
                {
                    foreach (var songAlert in song.Alerts.Where(a => a.Type == AlertType.File).ToList())
                    {
                        song.Alerts.Remove(songAlert);
                    }
                }

                await CheckSongFiles(fFmpegService, song, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);
                parsedSongIds.Add(song.Id);
                _logger.Info($"Song file '{songFile.FullName}' metadata stored in DB");
            }
            catch (Exception e)
            {
                _logger.Error($"There was an error during {songFile.FullName} analyze: {e}");
            }
        }

        private static async Task CheckSongFiles(IFFmpegService ffmpegService, DbSong song, CancellationToken cancellationToken)
        {
            await CheckSongFile(ffmpegService, song, FileType.Audio, cancellationToken);
            await CheckSongFile(ffmpegService, song, FileType.Video, cancellationToken);
            await CheckSongFile(ffmpegService, song, FileType.Cover, cancellationToken);
            await CheckSongFile(ffmpegService, song, FileType.Background, cancellationToken);
            await CheckSongFile(ffmpegService, song, FileType.Vocals, cancellationToken);
            await CheckSongFile(ffmpegService, song, FileType.Instrumental, cancellationToken);
        }

        private static async Task CheckSongFile(IFFmpegService ffmpegService, DbSong song,
            FileType fileType, CancellationToken cancellationToken)
        {
            if (!CheckSongFileExistence(song, fileType))
            {
                song.SetBrowserCompatibilityStatus(fileType, BrowserCompatibility.NotChecked);
                return;
            }

            await CheckSongFileBrowserCompatibility(ffmpegService, song, fileType, cancellationToken);
        }

        private static bool CheckSongFileExistence(DbSong song, FileType fileType)
        {
            var filePath = song.GetSongFilePath(fileType);
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (!song.SongFileExist(fileType))
            {
                song.Alerts.Add(new DbSongAlert
                {
                    Level = AlertLevel.Error,
                    Type = AlertType.File,
                    Message = $"The {fileType} file '{filePath}' doesn't exist on server"
                });
                return false;
            }

            return true;
        }

        private static async Task CheckSongFileBrowserCompatibility(IFFmpegService ffmpegService, DbSong song, FileType fileType, CancellationToken cancellationToken)
        {
            if (fileType is FileType.Cover or FileType.Background)
            {
                return;
            }

            BrowserCompatibility browserCompatibility;
            var filePath = song.GetSongFilePath(fileType);
            if (string.IsNullOrEmpty(filePath))
            {
                browserCompatibility = BrowserCompatibility.NotChecked;
            }
            else
            {
                if (fileType == FileType.Video)
                {
                    browserCompatibility = await ffmpegService.GetVideoCompatibilityAsync(filePath, cancellationToken);
                }
                else
                {
                    browserCompatibility = await ffmpegService.GetAudioCompatibilityAsync(filePath, cancellationToken);
                }
            }

            song.SetBrowserCompatibilityStatus(fileType, browserCompatibility);

            if (browserCompatibility != BrowserCompatibility.Compatible)
            {
                song.Alerts.Add(new DbSongAlert
                {
                    Level = browserCompatibility == BrowserCompatibility.ConversionMandatory ? AlertLevel.Error : AlertLevel.Warning,
                    Type = AlertType.File,
                    Message = browserCompatibility == BrowserCompatibility.ConversionMandatory
                        ? $"The {fileType} file is not compatible with WEB. Please convert it!"
                        : $"The {fileType} file may be not fully compatible with WEB. It's recommended to convert it"
                });
            }
        }
    }
}