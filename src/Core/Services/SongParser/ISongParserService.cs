using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Core.Persistence.Models.Songs;

namespace KaraW3B.Server.Core.Services.SongParser
{
    public interface ISongParserService
    {
        Task<bool> ParseSongAsync(FileInfo songFile, Song songToUpdate,
            CancellationToken cancellationToken);
    }
}