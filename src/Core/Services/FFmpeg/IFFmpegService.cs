using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Songs.Core.Models;

namespace KaraW3B.Server.Songs.Core.Services.FFmpeg
{
    public interface IFFmpegService
    {
        public Task<ConversionStatus> GetVideoCompatibilityAsync(string videoPath, CancellationToken cancellationToken);
        public Task<ConversionStatus> GetAudioCompatibilityAsync(string audioPath, CancellationToken cancellationToken);
    }
}
