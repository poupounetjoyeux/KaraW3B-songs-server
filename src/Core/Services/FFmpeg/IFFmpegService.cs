using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Core.Models;

namespace KaraW3B.Server.Core.Services.FFmpeg
{
    public interface IFFmpegService
    {
        public Task<ConversionStatus> GetVideoCompatibility(string videoPath, CancellationToken cancellationToken);
        public Task<ConversionStatus> GetAudioCompatibility(string audioPath, CancellationToken cancellationToken);
    }
}
