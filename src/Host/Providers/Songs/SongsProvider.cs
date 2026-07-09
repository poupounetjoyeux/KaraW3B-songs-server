using KaraW3B.SDK.Models.Songs;
using KaraW3B.SDK.Models.Songs.Files;
using KaraW3B.Server.Core.Persistence;
using KaraW3B.Server.Core.Persistence.Models.Songs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.SDK.Models.Songs.Alerts;

namespace KaraW3B.Server.Host.Providers.Songs
{
    internal sealed class SongsProvider : ISongsProvider
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider = new();
        private readonly KaraW3BDbContext _dbContext;

        public SongsProvider(KaraW3BDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async IAsyncEnumerable<SongDto> GetSongsByLibraryAsync(Guid libraryId, bool onlyLoadableSongs,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var song in _dbContext.Songs
                               .Where(s => s.LibraryId == libraryId)
                               .OrderBy(s => s.Artist)
                               .ThenBy(s => s.Title)
                               .ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (onlyLoadableSongs && song.Alerts.Any(a => a.Level == AlertLevel.Fatal))
                {
                    continue;
                }

                yield return song.ToDto();
            }
        }

        public Task<Song> GetSongById(Guid songId, CancellationToken cancellationToken)
        {
            return _dbContext.Songs.SingleOrDefaultAsync(s => s.Id == songId, cancellationToken);
        }

        public async Task<IActionResult> GetSongFileStream(Song song, FileType fileType,
            CancellationToken cancellationToken)
        {
            var filePath = song.GetSongFilePath(fileType);
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            if (!song.IsSongFileCompatible(fileType))
            {
                return new BadRequestObjectResult(
                    $"The file {fileType} for song {song.Id} isn't web player compatible");
            }

            var contentType = "application/octet-stream";
            if (_fileExtensionContentTypeProvider.TryGetContentType(filePath, out var gotContentType))
            {
                contentType = gotContentType;
            }

            return new PhysicalFileResult(filePath, contentType) { EnableRangeProcessing = true };
        }
    }
}