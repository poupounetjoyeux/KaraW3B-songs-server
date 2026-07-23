using System.Threading;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using KaraW3B.Server.Songs.Core.Helpers;
using KaraW3B.Server.Songs.Core.Models;
using KaraW3B.Server.Songs.Core.Services.Settings;
using System;

namespace KaraW3B.Server.Songs.Core.Services.FFmpeg
{
    public sealed class FFmpegService : IFFmpegService
    {
        private const string EncodedByTag = "encoded_by";

        public FFmpegService(ISettingsService settingsService)
        {
            var customFFmpegPath = settingsService.Settings.FFmpegPath;
            if (!string.IsNullOrEmpty(customFFmpegPath))
            {
                GlobalFFOptions.Configure(options => options.BinaryFolder = customFFmpegPath);
            }
        }

        public async Task<ConversionStatus> GetVideoCompatibilityAsync(string videoPath, CancellationToken cancellationToken)
        {
            var mediaInfos = await FFProbe.AnalyseAsync(videoPath, cancellationToken: cancellationToken);

            if ((mediaInfos.Format.Tags?.TryGetValue(EncodedByTag, out var value) ?? false) &&
                value.StartsWith(KaraW3BConstants.ApplicationName, StringComparison.OrdinalIgnoreCase))
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

            if (videoStream.CodecName != VideoCodec.LibX264.Name)
            {
                return ConversionStatus.Recommended;
            }

            return ConversionStatus.Compatible;
        }

        public async Task<ConversionStatus> GetAudioCompatibilityAsync(string audioPath, CancellationToken cancellationToken)
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
            public string Text => $"-metadata {EncodedByTag}=\"{KaraW3BConstants.ApplicationName}-v{KaraW3BConstants.ApplicationVersion}\"";
        }
    }
}
