using System;
using System.Collections.Generic;
using KaraW3B.SDK.Exceptions;
using KaraW3B.Server.Core.Helpers;
using KaraW3B.Server.Core.Persistence.Models.Songs;

namespace KaraW3B.Server.Core.Parsers
{
    internal sealed class UnversionedFormatParser : ParserBase
    {
        private int? _medleyStartBeat;
        private int? _medleyEndBeat;

        public UnversionedFormatParser(Song song) : base(song)
        {
        }

        protected override int BpmFactor => 4;

        protected override int AllowedNumberPlayers => 2;

        protected override Dictionary<string, string> DeprecatedHeaderAliases =>
            new() { { "DUETSINGERP1", "P1" }, { "DUETSINGERP2", "P2" } };

        protected override HashSet<string> VersionSpecificMandatoryHeaders => new() { "MP3" };

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
            if (headerName != "MP3")
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
                case "RELATIVE":
                    if (headerValue.Equals("YES", StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new KaraW3BException(
                            $"Relative mode is no more used in recent format versions.{Environment.NewLine}" +
                            $"Please avoid it since files order is less permissive and this program don't aim to support it.{Environment.NewLine}" +
                            $"You can always convert your old relative file by using UltraStar Deluxe song editor or UltraStar manager");
                    }

                    Warning("The #RELATIVE header is not enabled and can be removed", line);
                    return true;

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

        private void ComputeMedley()
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