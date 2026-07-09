using System.Threading;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using KaraW3B.SDK.Helpers;
using KaraW3B.Server.Core.Models;
using KaraW3B.Server.Core.Services.Settings;

namespace KaraW3B.Server.Core.Services.FFmpeg
{
    public sealed class FFmpegService : IFFmpegService
    {
        private const string EncodedByTag = "encoded_by";

        public FFmpegService(ISettingsService settingsService)
        {
            var customFFmpegPath = settingsService.GetSettingsAsync(CancellationToken.None).Result.FFmpegPath;
            if (!string.IsNullOrEmpty(customFFmpegPath))
            {
                GlobalFFOptions.Configure(options => options.BinaryFolder = customFFmpegPath);
            }
        }

        public async Task<ConversionStatus> GetVideoCompatibility(string videoPath, CancellationToken cancellationToken)
        {
            var mediaInfos = await FFProbe.AnalyseAsync(videoPath, cancellationToken: cancellationToken);

            if ((mediaInfos.Format.Tags?.TryGetValue(EncodedByTag, out var value) ?? false) &&
                value == KaraW3BConstants.ApplicationName)
            {
                return ConversionStatus.Compatible;
            }

            if (!mediaInfos.Format.FormatName.Contains("mp4"))
            {
                return ConversionStatus.Mandatory;
            }

            var videoStream = mediaInfos.PrimaryVideoStream;
            if (videoStream == null)
            {
                return ConversionStatus.Mandatory;
            }

            if (videoStream.PixelFormat != "yuv420p")
            {
                return ConversionStatus.Recommended;
            }

            if (videoStream.CodecName != VideoCodec.LibX264.Name)
            {
                return ConversionStatus.Recommended;
            }

            return ConversionStatus.Compatible;
        }

        public async Task<ConversionStatus> GetAudioCompatibility(string audioPath, CancellationToken cancellationToken)
        {
            var mediaInfos = await FFProbe.AnalyseAsync(audioPath, cancellationToken: cancellationToken);

            if ((mediaInfos.Format.Tags?.TryGetValue(EncodedByTag, out var value) ?? false) &&
                value == KaraW3BConstants.ApplicationName)
            {
                return ConversionStatus.Compatible;
            }

            if (mediaInfos.Format.FormatName != "mp3")
            {
                return ConversionStatus.Mandatory;
            }

            var audioStream = mediaInfos.PrimaryAudioStream;
            if (audioStream == null)
            {
                return ConversionStatus.Mandatory;
            }

            if (audioStream.CodecName != "mp3")
            {
                return ConversionStatus.Recommended;
            }

            return ConversionStatus.Compatible;
        }

        private class MovFlagsArgument : IArgument
        {
            private readonly string _flag;

            public MovFlagsArgument(string flag)
            {
                _flag = flag;
            }

            public string Text => $"-movflags {_flag}";
        }

        private class KaraW3BConvertedMetadataArgument : IArgument
        {
            public string Text => $"-metadata {EncodedByTag}=\"{KaraW3BConstants.ApplicationName}\"";
        }
    }
}
