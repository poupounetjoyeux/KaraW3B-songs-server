using KaraWeb.Core.Persistence.Models.Songs;
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
        IAsyncEnumerable<SongDto> GetSongsByLibraryAsync(Guid libraryId, bool onlyLoadableSongs,
            CancellationToken cancellationToken);

        Task<Song> GetSongById(Guid songId, CancellationToken cancellationToken);
        Task<PhysicalFileResult> GetSongFileStream(Song song, FileType fileType, CancellationToken cancellationToken);
    }
}