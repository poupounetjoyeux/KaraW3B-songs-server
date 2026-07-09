using System;
using System.Collections.Generic;
using KaraW3B.Server.Core.Helpers;
using KaraW3B.Server.Core.Persistence.Models.Songs;

namespace KaraW3B.Server.Core.Parsers
{
    internal class V1FormatParser : ParserBase
    {
        private int? _medleyStartBeat;
        private int? _medleyEndBeat;

        public V1FormatParser(Song song) : base(song)
        {
        }

        protected override int BpmFactor => 4;

        protected override int AllowedNumberPlayers => 9;

        protected override Dictionary<string, string> DeprecatedHeaderAliases =>
            new() { { "MP3", "AUDIO" }, { "INSTRUMENTALS", "INSTRUMENTAL" } };

        protected override HashSet<string> VersionSpecificMandatoryHeaders => new() { "AUDIO", "VERSION" };

        protected override Dictionary<string, Func<double, TimeSpan>> TimeHeaderFactories => new()
        {
            { "GAP", TimeSpan.FromMilliseconds },
            { "VIDEOGAP", TimeSpan.FromSeconds },
            { "PREVIEWSTART", TimeSpan.FromSeconds },
            { "START", TimeSpan.FromSeconds },
            { "END", TimeSpan.FromMilliseconds }
        };

        protected override bool HandleSpecificVersionCoreHeader(string headerName, string headerValue, int line)
        {
            if (headerName != "AUDIO")
            {
                return false;
            }

            Song.Audio = headerValue;
            return true;
        }

        protected override bool HandleSpecificVersionExtraHeader(string headerName, string headerValue, int line)
        {
            switch (headerName)
            {
                case "AUDIOURL":
                    Song.AudioUrl = headerValue;
                    return true;

                case "VIDEOURL":
                    Song.VideoUrl = headerValue;
                    return true;

                case "COVERURL":
                    Song.CoverUrl = headerValue;
                    return true;

                case "BACKGROUNDURL":
                    Song.BackgroundUrl = headerValue;
                    return true;

                case "PROVIDEDBY":
                    Song.ProvidedBy = headerValue;
                    return true;

                case "TAGS":
                    ParseListHeaderValue(headerValue, Song.Tags);
                    break;

                case "VOCALS":
                    Song.Vocals = headerValue;
                    return true;

                case "INSTRUMENTAL":
                    Song.Instrumental = headerValue;
                    return true;

                case "RENDITION":
                    Song.Rendition = headerValue;
                    return true;
            }

            return HandleMedleyHeader(headerName, headerValue, line);
        }

        protected virtual bool HandleMedleyHeader(string headerName, string headerValue, int line)
        {
            switch (headerName)
            {
                case "MEDLEYSTARTBEAT":
                    ParseIntHeaderValue(headerName, headerValue, line, startBeat => _medleyStartBeat = startBeat);
                    return true;

                case "MEDLEYENDBEAT":
                    ParseIntHeaderValue(headerName, headerValue, line, endBeat => _medleyEndBeat = endBeat);
                    return true;
            }

            return false;
        }

        public override void PostParsing()
        {
            ComputeMedley();
        }

        protected virtual void ComputeMedley()
        {
            if (!_medleyStartBeat.HasValue && !_medleyEndBeat.HasValue)
            {
                return;
            }

            if (!_medleyStartBeat.HasValue)
            {
                Error("#MEDLEYSTARTBEAT is mandatory when #MEDLEYENDBEAT is specified");
                return;
            }

            if (!_medleyEndBeat.HasValue)
            {
                Error("#MEDLEYENDBEAT is mandatory when #MEDLEYSTARTBEAT is specified");
                return;
            }

            ParsingHelper.ComputeMedleyTimesFromBeats(Song, _medleyStartBeat.Value, _medleyEndBeat.Value);
        }
    }
}