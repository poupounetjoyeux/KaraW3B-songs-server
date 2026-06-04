using KaraWeb.Core.Persistence.Songs;
using KaraWeb.Shared.Models.Songs;
using KaraWeb.Shared.Models.Songs.Files;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KaraWeb.Host.Providers.Songs
{
    public interface ISongsProvider
    {
        IAsyncEnumerable<SongDto> GetSongsByLibraryAsync(Guid libraryId, bool withErrors,
            CancellationToken cancellationToken);
        Task<DetailedSongDto> GetDetailedSongAsync(Guid songId, CancellationToken cancellationToken);
        Task<Song> GetSongById(Guid songId, CancellationToken cancellationToken);
        Task<FileStreamResult> GetSongFileStream(Song song, SongFileType fileType, CancellationToken cancellationToken);
    }
}