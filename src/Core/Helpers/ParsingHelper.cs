using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using KaraW3B.SDK.Helpers;
using KaraW3B.SDK.Models.Analyzes;
using KaraW3B.SDK.Models.Songs.Alerts;
using KaraW3B.SDK.Models.Songs.Notes;
using KaraW3B.Server.Core.Persistence.Models.Songs;

namespace KaraW3B.Server.Core.Helpers
{
    internal static class ParsingHelper
    {
        public static readonly HashSet<string> DefaultMandatoryHeaders = new()
        {
            "TITLE",
            "ARTIST",
            "BPM"
        };

        public static readonly Dictionary<string, string> DefaultHeaderAliases = new()
        {
            { "AUTHOR", "CREATOR" },
            { "PREVIEW", "PREVIEWSTART" }
        };

        public const int MaxRecommendedHeaderSize = 2048;
        public const char ListSplitter = ',';

        #region Common regex

        public static readonly Regex HeaderRegex =
            new("^#(?<headerName>[A-Z0-9]+): *(?<headerValue>.*) *$",
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static readonly Regex NoteRegex =
            new(@"^(?<noteType>.) (?<startBeat>\d+) (?<duration>\d+) (?<pitch>-?\d+) (?<text>.*)$",
                RegexOptions.Compiled | RegexOptions.Singleline);

        public static readonly Regex EndOfPhraseRegex =
            new(@"^-( (?<startBeat>\d+))?",
                RegexOptions.Compiled | RegexOptions.Singleline);

        // Space is for yass :(
        public static readonly Regex PlayerNumberRegex =
            new("^P *(?<playerNumber>[1-9])$",
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        #endregion

        public static void AddParsingFatal(this Song song, string message, int? line = null)
        {
            song.AddParsingAlert(AlertLevel.Fatal, message, line);
        }

        public static void AddParsingError(this Song song, string message, int? line = null)
        {
            song.AddParsingAlert(AlertLevel.Error, message, line);
        }

        public static void AddParsingWarning(this Song song, string message, int? line = null)
        {
            song.AddParsingAlert(AlertLevel.Warning, message, line);
        }

        private static void AddParsingAlert(this Song song, AlertLevel level, string message, int? line)
        {
            song.Alerts.Add(new SongAlert
                { Type = AlertType.Parsing, Level = level, Message = message, FileLine = line });
        }

        public static void AddAnalyzeAlerts(this Song song, FullAnalyzeResult analyzeResult)
        {
            foreach (var infoError in analyzeResult.InfoErrors)
            {
                song.Alerts.Add(new SongAlert
                {
                    Type = AlertType.Info,
                    Level = infoError.IsWarning ? AlertLevel.Warning : AlertLevel.Error,
                    Message = infoError.Message
                });
            }

            foreach (var noteError in analyzeResult.NotesErrors)
            {
                song.Alerts.Add(new SongAlert
                {
                    Type = AlertType.Note,
                    Level = AlertLevel.Error,
                    Message = noteError.Message,
                    FileLine = noteError.FileLine
                });
            }
        }

        public static NoteType ParseNoteType(string noteType)
        {
            return noteType.ToUpperInvariant() switch
            {
                ":" => NoteType.Regular,
                "*" => NoteType.Golden,
                "R" => NoteType.Rap,
                "G" => NoteType.GoldenRap,
                _ => NoteType.Freestyle
            };
        }

        public static void ComputeMedleyTimesFromBeats(Song song, int medleyStartBeat, int medleyEndBeat)
        {
            var medleyStartTime = TimesHelper.GetTimeFromBeat(song.Bpm, medleyStartBeat, song.Gap);
            if (!medleyStartTime.HasValue)
            {
                song.AddParsingError("Unable to compute the medley start time");
                return;
            }

            var medleyEndTime = TimesHelper.GetTimeFromBeat(song.Bpm, medleyEndBeat, song.Gap);
            if (!medleyEndTime.HasValue)
            {
                song.AddParsingError("Unable to compute the medley end time");
                return;
            }

            song.Medley = new SongMedley
            {
                MedleyStart = medleyStartTime.Value,
                MedleyEnd = medleyEndTime.Value
            };
        }

        public static bool TryExtractPlayerNumber(string text, out int playerNumber)
        {
            var playerHeaderMatch = PlayerNumberRegex.Match(text);
            if (!playerHeaderMatch.Success)
            {
                playerNumber = 0;
                return false;
            }

            playerNumber = int.Parse(playerHeaderMatch.Groups["playerNumber"].Value, CultureInfo.InvariantCulture);
            return true;
        }
    }
}