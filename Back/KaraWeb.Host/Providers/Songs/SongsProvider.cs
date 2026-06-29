using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KaraWeb.Core.Persistence;
using KaraWeb.Core.Persistence.Models.Songs;
using KaraWeb.Shared.Models.Songs;
using KaraWeb.Shared.Models.Songs.Files;
using KaraWeb.Shared.Models.Songs.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace KaraWeb.Host.Providers.Songs
{
    internal sealed class SongsProvider : ISongsProvider
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider = new();
        private readonly KaraWebDbContext _dbContext;

        public SongsProvider(KaraWebDbContext dbContext)
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

        public Task<PhysicalFileResult> GetSongFileStream(Song song, FileType fileType,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var filePath = song.GetSongFilePath(fileType);
                if (string.IsNullOrEmpty(filePath))
                {
                    return null;
                }

                var contentType = "application/octet-stream";
                if (_fileExtensionContentTypeProvider.TryGetContentType(filePath, out var gotContentType))
                {
                    contentType = gotContentType;
                }

                return new PhysicalFileResult(filePath, contentType) { EnableRangeProcessing = true };
            }, cancellationToken);
        }
    }
}