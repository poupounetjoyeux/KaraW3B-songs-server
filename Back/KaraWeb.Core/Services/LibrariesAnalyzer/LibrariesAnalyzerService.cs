using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using KaraWeb.Core.Persistence;
using KaraWeb.Core.Services.SongParser;
using KaraWeb.Shared.Models.Libraries;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace KaraWeb.Core.Services.LibrariesAnalyzer
{
    public sealed class LibrariesAnalyzerService : ILibrariesAnalyzerService
    {
        private readonly ILog _logger = LogManager.GetLogger(nameof(LibrariesAnalyzerService));
        private readonly ISongParserService _songParserService;

        public LibrariesAnalyzerService(ISongParserService songParserService)
        {
            _songParserService = songParserService;
        }

        public async Task StartLibraryAnalyzeAsync(IAnalyzableLibrary library, LibraryAnalyzeType analyzeType,
            CancellationToken cancellationToken)
        {
            // TODO: Make it background
            var directory = new DirectoryInfo(library.Path);
            if (!directory.Exists)
            {
                _logger.Error($"Directory '{directory.FullName}' doesn't exist");
                return;
            }

            _logger.Info($"Start analyzing library '{library.Name}' in mode '{analyzeType}'");
            var timeWatcher = new Stopwatch();
            timeWatcher.Start();

            var foundFiles = directory.GetFiles("*.txt", SearchOption.AllDirectories);
            _logger.Info($"Found {foundFiles.Length} potential song file(s) to analyze");
            var parsedSongIds = new ConcurrentBag<Guid>();
            await Parallel.ForEachAsync(foundFiles, cancellationToken,
                (f, c) => ParseSongFile(library.Id, analyzeType, parsedSongIds, f, c));

            await using var dbContext = new KaraWebDbContext();

            var songsToDelete =
                await dbContext.Songs.Where(s => !parsedSongIds.Contains(s.Id)).ToListAsync(cancellationToken);
            dbContext.RemoveRange(songsToDelete);
            await dbContext.SaveChangesAsync(cancellationToken);

            timeWatcher.Stop();
            _logger.Info(
                $"LibraryDto {library.Name} parsed {parsedSongIds.Count} songs and deleted {songsToDelete.Count} songs successfully in {timeWatcher.Elapsed}");
        }

        private static async Task<string> ComputeFileHash(FileInfo file, CancellationToken cancellationToken)
        {
            var fileBytes = await File.ReadAllBytesAsync(file.FullName, cancellationToken);
            var hashBytes = SHA1.HashData(fileBytes);
            return Convert.ToHexStringLower(hashBytes);
        }

        private async ValueTask ParseSongFile(Guid libraryId, LibraryAnalyzeType analyzeType,
            ConcurrentBag<Guid> parsedSongIds, FileInfo songFile, CancellationToken cancellationToken)
        {
            _logger.Info($"Starting analyze of song file '{songFile.FullName}'");
            await using var dbContext = new KaraWebDbContext();
            var existingSong = await dbContext.Songs.SingleOrDefaultAsync(s =>
                s.SongFilePath == songFile.FullName &&
                s.LibraryId == libraryId, cancellationToken);

            var fileHash = await ComputeFileHash(songFile, cancellationToken);
            if (analyzeType == LibraryAnalyzeType.Optimized)
            {
                if (existingSong != null && existingSong.AnalyzedFileHash == fileHash)
                {
                    _logger.Info($"Same hash already in DB for file '{songFile.FullName}'");
                    parsedSongIds.Add(existingSong.Id);
                    return;
                }
            }

            var parsedSong = await _songParserService.ParseSongAsync(libraryId, songFile, fileHash, cancellationToken);
            if (parsedSong != null)
            {
                Guid id;
                if (existingSong != null)
                {
                    id = existingSong.Id;
                    parsedSong.Id = id;
                    dbContext.Entry(existingSong).CurrentValues.SetValues(parsedSong);
                }
                else
                {
                    var persistedSongEntity = await dbContext.Songs.AddAsync(parsedSong, cancellationToken);
                    id = persistedSongEntity.Entity.Id;
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                parsedSongIds.Add(id);
                _logger.Info($"Song file '{songFile.FullName}' metadata stored in DB");
            }
        }
    }
}