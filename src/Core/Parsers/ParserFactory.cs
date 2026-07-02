using KaraW3B.SDK.Exceptions;
using KaraW3B.Server.Core.Persistence.Models.Songs;

namespace KaraW3B.Server.Core.Parsers
{
    internal static class ParserFactory
    {
        public static ParserBase GetParser(this Song song)
        {
            if (song.Version == null)
            {
                return new UnversionedFormatParser(song);
            }

            if (song.Version.Major == 1)
            {
                return new V1FormatParser(song);
            }

            if (song.Version.Major == 2)
            {
                return new V2FormatParser(song);
            }

            throw new KaraW3BException($"$The version {song.Version.ToString(3)} has no parser implementation");
        }
    }
}