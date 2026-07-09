using KaraW3B.SDK.Models.Songs;
using KaraW3B.SDK.Models.Songs.Files;
using KaraW3B.Server.Core.Persistence.Models.Songs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KaraW3B.Server.Host.Providers.Songs
{
    public interface ISongsProvider
    {
        IAsyncEnumerable<SongDto> GetSongsByLibraryAsync(Guid libraryId, bool onlyLoadableSongs,
            CancellationToken cancellationToken);

        Task<Song> GetSongById(Guid songId, CancellationToken cancellationToken);
        Task<IActionResult> GetSongFileStream(Song song, FileType fileType, CancellationToken cancellationToken);
    }
}