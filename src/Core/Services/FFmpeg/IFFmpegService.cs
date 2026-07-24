using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Songs.Core.Models;
using KaraW3B.Server.Songs.Models.Songs;

namespace KaraW3B.Server.Songs.Core.Services.FFmpeg
{
    public interface IFFmpegService
    {
        public Task<BrowserCompatibility> GetVideoCompatibilityAsync(string videoPath, CancellationToken cancellationToken);
        public Task<BrowserCompatibility> GetAudioCompatibilityAsync(string audioPath, CancellationToken cancellationToken);
    }
}
