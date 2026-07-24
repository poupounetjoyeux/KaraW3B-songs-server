using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Songs.Core.Persistence;
using KaraW3B.Server.Songs.Core.Persistence.Models.Songs;
using KaraW3B.Server.Songs.Models.Songs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace KaraW3B.Server.Songs.Host.Providers.Songs
{
    internal sealed class SongsProvider : ISongsProvider
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider = new();
        private readonly KaraW3BDbContext _dbContext;

        public SongsProvider(KaraW3BDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async IAsyncEnumerable<Song> GetSongsByLibraryAsync(Guid libraryId, bool onlyLoadableSongs,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var song in _dbContext.Songs
                               .Where(s => s.LibraryId == libraryId)
                               .OrderBy(s => s.Artist)
                               .ThenBy(s => s.Title)
                               .ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (onlyLoadableSongs && song.IsNotLoadable())
                {
                    continue;
                }

                yield return song.ToSong();
            }
        }

        public Task<DbSong> GetSongById(Guid songId, CancellationToken cancellationToken)
        {
            return _dbContext.Songs.SingleOrDefaultAsync(s => s.Id == songId, cancellationToken);
        }

        public async Task<PhysicalFileResult> GetSongFileStream(DbSong song, FileType fileType,
            CancellationToken cancellationToken)
        {
            var filePath = song.GetSongFilePath(fileType);

            var contentType = "application/octet-stream";
            if (_fileExtensionContentTypeProvider.TryGetContentType(filePath, out var gotContentType))
            {
                contentType = gotContentType;
            }

            return new PhysicalFileResult(filePath, contentType) { EnableRangeProcessing = true };
        }
    }
}