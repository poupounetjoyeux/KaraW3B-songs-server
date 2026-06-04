using KaraWeb.Core.Persistence;
using KaraWeb.Shared.Models.Songs;
using KaraWeb.Shared.Models.Songs.Files;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KaraWeb.Core.Persistence.Songs;
using Microsoft.AspNetCore.Mvc;

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

        public async IAsyncEnumerable<SongDto> GetSongsByLibraryAsync(Guid libraryId, bool withErrors,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var song in _dbContext.Songs
                               .Where(s => s.LibraryId == libraryId && (withErrors || s.Errors.Count == 0))
                               .ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return song.ToDto();
            }
        }

        public async Task<DetailedSongDto> GetDetailedSongAsync(Guid songId, CancellationToken cancellationToken)
        {
            return (await GetSongById(songId, cancellationToken)).ToDetailedDto();
        }

        public Task<Song> GetSongById(Guid songId, CancellationToken cancellationToken)
        {
            return _dbContext.Songs.SingleOrDefaultAsync(s => s.Id == songId, cancellationToken);
        }

        public Task<FileStreamResult> GetSongFileStream(Song song, SongFileType fileType, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var songDirectory = Path.GetDirectoryName(song.SongFilePath);
                if (songDirectory == null)
                {
                    return null;
                }

                var filePath = fileType switch
                {
                    SongFileType.Audio => song.Audio,
                    SongFileType.Cover => song.Cover,
                    SongFileType.Background => song.Background,
                    SongFileType.Video => song.Video,
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(filePath))
                {
                    return null;
                }

                filePath = Path.Combine(songDirectory, filePath);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                var contentType = "application/octet-stream";
                if (_fileExtensionContentTypeProvider.TryGetContentType(filePath, out var gotContentType))
                {
                    contentType = gotContentType;
                }

                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return new FileStreamResult(stream, contentType) { FileDownloadName = Path.GetFileName(filePath) };
            }, cancellationToken);
        }
    }
}