using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KaraW3B.SDK.Exceptions;
using KaraW3B.SDK.Models.Songs.Notes;
using KaraW3B.Server.Core.Helpers;
using KaraW3B.Server.Core.Persistence.Models.Songs;

namespace KaraW3B.Server.Core.Parsers
{
    internal abstract class ParserBase
    {
        private readonly HashSet<string> _parsedHeaders = new();
        private int _currentPlayer;

        protected Song Song { get; }

        protected ParserBase(Song song)
        {
            Song = song;
        }

        #region Abstractions

        protected abstract int BpmFactor { get; }

        protected abstract int AllowedNumberPlayers { get; }

        protected abstract Dictionary<string, string> DeprecatedHeaderAliases { get; }

        protected abstract HashSet<string> VersionSpecificMandatoryHeaders { get; }

        protected abstract Dictionary<string, Func<double, TimeSpan>> TimeHeaderFactories { get; }

        protected abstract bool HandleSpecificVersionCoreHeader(string headerName, string headerValue, int line);

        protected abstract bool HandleSpecificVersionExtraHeader(string headerName, string headerValue, int line);

        public abstract void PostParsing();

        #endregion

        #region Utils

        protected void Fatal(string message, int? line = null)
        {
            Song.AddParsingFatal(message, line);
        }

        protected void Error(string message, int? line = null)
        {
            Song.AddParsingError(message, line);
        }

        protected void Warning(string message, int? line = null)
        {
            Song.AddParsingWarning(message, line);
        }

        protected void ParseDecimalHeaderValue(string headerName, string headerValue, int line, Action<decimal> setter)
        {
            if (!decimal.TryParse(headerValue.Replace(',', '.'), CultureInfo.InvariantCulture,
                    out var parsedDecimalValue))
            {
                Error($"Unable to parse #{headerName} header, it must be a decimal", line);
                return;
            }

            setter(parsedDecimalValue);
        }

        protected void ParseIntHeaderValue(string headerName, string headerValue, int line, Action<int> setter)
        {
            if (!int.TryParse(headerValue, CultureInfo.InvariantCulture, out var parsedIntValue))
            {
                Error($"Unable to parse #{headerName} header, it must be an integer", line);
                return;
            }

            setter(parsedIntValue);
        }

        protected void ParseTimeHeaderValue(string headerName, string headerValue, int line, Action<TimeSpan> setter)
        {
            if (TimeHeaderFactories == null || !TimeHeaderFactories.TryGetValue(headerName, out var timeFactory))
            {
                throw new KaraW3BException(
                    $"No time factory defined in parser {GetType().Name} for header #{headerName}");
            }

            try
            {
                var doubleValue = double.Parse(headerValue.Replace(',', '.'), CultureInfo.InvariantCulture);
                var timeSpan = timeFactory(doubleValue);
                setter(timeSpan);
            }
            catch (Exception)
            {
                Error($"Unable to parse #{headerName} header, it must be a time in decimal", line);
            }
        }

        protected static void ParseListHeaderValue(string headerValue, List<string> values)
        {
            if (string.IsNullOrEmpty(headerValue))
            {
                return;
            }

            values.AddRange(headerValue
                .Split(new[] { ParsingHelper.ListSplitter }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim()));
        }

        #endregion

        #region Headers

        public void CheckMandatoryHeadersAreDefined()
        {
            IEnumerable<string> mandatoryHeaders = ParsingHelper.DefaultMandatoryHeaders;
            if (VersionSpecificMandatoryHeaders != null)
            {
                mandatoryHeaders = mandatoryHeaders.Concat(VersionSpecificMandatoryHeaders).Distinct();
            }

            foreach (var mandatoryHeader in mandatoryHeaders)
            {
                if (_parsedHeaders.Contains(mandatoryHeader))
                {
                    continue;
                }

                Fatal($"The mandatory #{mandatoryHeader} header is missing");
            }
        }

        public bool TryParseFileHeaderLine(string fileLine, int line)
        {
            var headerLineMatch = ParsingHelper.HeaderRegex.Match(fileLine);
            if (!headerLineMatch.Success)
            {
                return false;
            }

            var headerName = headerLineMatch.Groups["headerName"].Value.ToUpperInvariant();
            var headerValue = headerLineMatch.Groups["headerValue"].Value;

            if (!IsHeaderValid(headerName, headerValue, line))
            {
                return true;
            }

            string deprecatedHeaderName = null;
            if (ParsingHelper.DefaultHeaderAliases.TryGetValue(headerName, out var replacementHeader)
                || (DeprecatedHeaderAliases != null &&
                    DeprecatedHeaderAliases.TryGetValue(headerName, out replacementHeader)))
            {
                Warning($"The header #{headerName} is deprecated and should be replaced by #{replacementHeader}", line);
                deprecatedHeaderName = headerName;
                headerName = replacementHeader;
            }

            if (!_parsedHeaders.Add(headerName))
            {
                Error(deprecatedHeaderName != null
                    ? $"The deprecated header #{deprecatedHeaderName} is duplicated with #{headerName} you must remove it"
                    : $"The header #{headerName} is duplicated", line);
                return true;
            }

            if (HandleCommonCoreHeader(headerName, headerValue, line))
            {
                return true;
            }

            if (HandleSpecificVersionCoreHeader(headerName, headerValue, line))
            {
                return true;
            }

            if (HandleCommonExtraHeader(headerName, headerValue, line))
            {
                return true;
            }

            if (HandleSpecificVersionExtraHeader(headerName, headerValue, line))
            {
                return true;
            }

            if (HandlePlayerHeader(headerName, headerValue, line))
            {
                return true;
            }

            Song.NotManagedHeaders.Add($"{headerName}={headerValue}");
            return true;
        }

        private bool IsHeaderValid(string headerName, string headerValue, int line)
        {
            if (string.IsNullOrEmpty(headerValue))
            {
                Warning($"The header #{headerName} has no value and must be removed", line);
                return false;
            }

            if (headerName.Length > ParsingHelper.MaxRecommendedHeaderSize)
            {
                Warning(
                    $"The header #{headerName} length is greater than {ParsingHelper.MaxRecommendedHeaderSize} bytes",
                    line);
            }

            if (headerValue.Length > ParsingHelper.MaxRecommendedHeaderSize)
            {
                Warning(
                    $"The header #{headerName} has a value greater than {ParsingHelper.MaxRecommendedHeaderSize} bytes",
                    line);
            }

            return true;
        }

        private bool HandleCommonCoreHeader(string headerName, string headerValue, int line)
        {
            switch (headerName)
            {
                case "BPM":
                    ParseDecimalHeaderValue(headerName, headerValue, line, bpm => Song.Bpm = bpm * BpmFactor);
                    return true;

                case "TITLE":
                    Song.Title = headerValue;
                    return true;

                case "ARTIST":
                    Song.Artist = headerValue;
                    return true;

                case "GAP":
                    ParseTimeHeaderValue(headerName, headerValue, line, gap => Song.Gap = gap);
                    return true;

                case "START":
                    ParseTimeHeaderValue(headerName, headerValue, line, start => Song.Start = start);
                    return true;

                case "END":
                    ParseTimeHeaderValue(headerName, headerValue, line, end => Song.End = end);
                    return true;
            }

            return false;
        }

        private bool HandleCommonExtraHeader(string headerName, string headerValue, int line)
        {
            switch (headerName)
            {
                case "COVER":
                    Song.Cover = headerValue;
                    return true;

                case "BACKGROUND":
                    Song.Background = headerValue;
                    return true;

                case "VIDEO":
                    Song.Video = headerValue;
                    return true;

                case "VIDEOGAP":
                    ParseTimeHeaderValue(headerName, headerValue, line, videoGap => Song.VideoGap = videoGap);
                    return true;

                case "PREVIEWSTART":
                    ParseTimeHeaderValue(headerName, headerValue, line,
                        previewStart => Song.PreviewStart = previewStart);
                    return true;

                case "YEAR":
                    if (headerValue.Length != 4 ||
                        !int.TryParse(headerValue, CultureInfo.InvariantCulture, out var year) || year < 1)
                    {
                        Error("#YEAR header is not a valid year with format YYYY", line);
                    }
                    else
                    {
                        Song.Year = year;
                    }

                    return true;

                case "GENRE":
                    ParseListHeaderValue(headerValue, Song.Genres);
                    return true;

                case "LANGUAGE":
                    ParseListHeaderValue(headerValue, Song.Languages);
                    return true;

                case "EDITION":
                    ParseListHeaderValue(headerValue, Song.Editions);
                    return true;

                case "CREATOR":
                    ParseListHeaderValue(headerValue, Song.Creators);
                    return true;

                case "COMMENT":
                    Song.Comment = headerValue;
                    return true;
            }

            return false;
        }

        private bool HandlePlayerHeader(string headerName, string headerValue, int line)
        {
            if (!ParsingHelper.TryExtractPlayerNumber(headerName, out var playerNumber))
            {
                return false;
            }

            if (playerNumber < 1)
            {
                Error($"Player header #{headerName} is invalid. Player number must be at least 1", line);
                return false;
            }

            if (playerNumber > AllowedNumberPlayers)
            {
                Error(
                    $"Player header #{headerName} is invalid. Maximum {AllowedNumberPlayers} player(s) can be declared",
                    line);
                return true;
            }

            Song.Players.Add(new SongPlayer { Number = playerNumber, Name = headerValue });
            return true;
        }

        #endregion

        #region Notes

        public bool TryParseFileNoteLine(string fileLine, int line)
        {
            var newNote = TryParseEndOfPhraseNote(fileLine, line) ??
                          TryParseNote(fileLine, line);
            if (newNote != null)
            {
                Song.Notes.Add(newNote);
                return true;
            }

            if (!ParsingHelper.TryExtractPlayerNumber(fileLine, out var newPlayerNumber))
            {
                return false;
            }

            if (newPlayerNumber < 1)
            {
                throw new KaraW3BException("Player number must be at least 1");
            }

            if (newPlayerNumber > AllowedNumberPlayers)
            {
                throw new KaraW3BException($"Maximum {AllowedNumberPlayers} player(s) can be declared");
            }

            _currentPlayer = newPlayerNumber;
            return true;
        }

        private SongNote TryParseEndOfPhraseNote(string fileLine, int currentLine)
        {
            var endOfPhraseMatch = ParsingHelper.EndOfPhraseRegex.Match(fileLine);
            if (!endOfPhraseMatch.Success)
            {
                return null;
            }

            var note = new SongNote
            {
                FileLine = currentLine,
                PlayerNumber = _currentPlayer,
                Type = NoteType.EndOfPhrase,
                StartBeat = -1,
                Duration = null
            };

            if (endOfPhraseMatch.Groups["startBeat"].Success)
            {
                note.StartBeat = int.Parse(endOfPhraseMatch.Groups["startBeat"].Value, CultureInfo.InvariantCulture);
            }
            else
            {
                var previousNote = Song.Notes.Where(n => n.PlayerNumber == _currentPlayer).MaxBy(n => n.FileLine);
                if (previousNote == null)
                {
                    return note;
                }

                var duration = 1;
                if (previousNote.Duration.HasValue)
                {
                    duration = previousNote.Duration.Value;
                }

                note.StartBeat = previousNote.StartBeat + duration - 1;
            }

            return note;
        }

        private SongNote TryParseNote(string fileLine, int currentLine)
        {
            var noteMatch = ParsingHelper.NoteRegex.Match(fileLine);
            if (!noteMatch.Success)
            {
                return null;
            }

            return new SongNote
            {
                FileLine = currentLine,
                PlayerNumber = _currentPlayer,
                Type = ParsingHelper.ParseNoteType(noteMatch.Groups["noteType"].Value),
                StartBeat = int.Parse(noteMatch.Groups["startBeat"].Value, CultureInfo.InvariantCulture),
                Duration = int.Parse(noteMatch.Groups["duration"].Value, CultureInfo.InvariantCulture),
                Pitch = int.Parse(noteMatch.Groups["pitch"].Value, CultureInfo.InvariantCulture),
                Text = noteMatch.Groups["text"].Value
            };
        }

        #endregion
    }
}