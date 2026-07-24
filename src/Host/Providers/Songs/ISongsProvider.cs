using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Songs.Core.Persistence.Models.Songs;
using KaraW3B.Server.Songs.Models.Songs;
using Microsoft.AspNetCore.Mvc;

namespace KaraW3B.Server.Songs.Host.Providers.Songs
{
    public interface ISongsProvider
    {
        IAsyncEnumerable<Song> GetSongsByLibraryAsync(Guid libraryId, bool onlyLoadableSongs,
            CancellationToken cancellationToken);

        Task<DbSong> GetSongById(Guid songId, CancellationToken cancellationToken);
        Task<PhysicalFileResult> GetSongFileStream(DbSong song, FileType fileType, CancellationToken cancellationToken);
    }
}