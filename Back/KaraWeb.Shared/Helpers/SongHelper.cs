using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KaraWeb.Shared.Models;
using KaraWeb.Shared.Models.Songs;
using KaraWeb.Shared.Models.Songs.Notes;

namespace KaraWeb.Shared.Helpers
{
    /// <summary>
    ///     Song error checkers are based on https://github.com/UltraStar-Deluxe/format and https://usdx.eu/format/
    ///     They aim to be able to not reject the maximum of files wherever they are up to date or not
    ///     The goal is also to add recommendations to be compatible with the more recent format
    /// </summary>
    public static class SongHelper
    {
        public static Task<ErrorsAnalyzeResult> CheckHeadersErrorsAsync(IAnalyzableSong song, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var result = new ErrorsAnalyzeResult();
                if (!string.IsNullOrEmpty(song.Encoding))
                {
                    result.Warnings.Add(!EncodingHelper.IsDefaultEncoding(song.Encoding)
                        ? "Prefer using UTF8 (without BOM) for your files encoding"
                        : "When song encoding is already in UTF8, #ENCODING header should be removed");
                }

                if (string.IsNullOrEmpty(song.Title))
                {
                    result.Errors.Add("A title is mandatory (#TITLE header)");
                }

                if (string.IsNullOrEmpty(song.Artist))
                {
                    result.Errors.Add("An artist is mandatory (#ARTIST header)");
                }

                if (!song.Bpm.HasValue)
                {
                    result.Errors.Add("The song BPM is mandatory (#BPM header)");
                }
                else if (song.Bpm.Value < 1)
                {
                    result.Errors.Add("The song BPM is less than 1");
                }

                if (string.IsNullOrEmpty(song.Audio))
                {
                    result.Errors.Add("The song audio file is mandatory (#AUDIO header)");
                }
                else if (Path.IsPathFullyQualified(song.Audio))
                {
                    result.Errors.Add("The song audio file must be a relative path");
                }

                if (!string.IsNullOrEmpty(song.Video) && Path.IsPathFullyQualified(song.Video))
                {
                    result.Errors.Add("The song video file must be a relative path");
                }

                if (!string.IsNullOrEmpty(song.Cover) && Path.IsPathFullyQualified(song.Cover))
                {
                    result.Errors.Add("The song cover file must be a relative path");
                }

                if (!string.IsNullOrEmpty(song.Background) && Path.IsPathFullyQualified(song.Background))
                {
                    result.Errors.Add("The song background file must be a relative path");
                }

                foreach (var songPlayer in song.GetPlayers())
                {
                    if (string.IsNullOrEmpty(songPlayer.Value))
                    {
                        result.Errors.Add(
                            $"The player {songPlayer.Key} has no name defined (#P{songPlayer.Key} header)");
                    }
                }

                return result;
            }, cancellationToken);
        }

        public static Task<ErrorsAnalyzeResult> CheckNotesErrorsAsync(IAnalyzableSong song, IEnumerable<IAnalyzableSongNote> songNotes,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var result = new ErrorsAnalyzeResult();
                var processedPlayerBeats = new Dictionary<int, HashSet<int>>();

                var hasNotes = false;
                foreach (var note in songNotes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    hasNotes = true;
                    if (note.StartBeat < 1)
                    {
                        result.Errors.Add(
                            $"Note {note.StartBeat} for player {note.PlayerNumber} must start at least at beat 1");
                    }

                    if (!processedPlayerBeats.TryGetValue(note.PlayerNumber, out var playerProcessedBeats))
                    {
                        playerProcessedBeats = new HashSet<int>(note.StartBeat);
                        processedPlayerBeats.Add(note.PlayerNumber, playerProcessedBeats);
                    }
                    else if (playerProcessedBeats.Contains(note.StartBeat))
                    {
                        result.Errors.Add(
                            $"There is a duplicated note on beat {note.StartBeat} for player {note.PlayerNumber}");
                    }

                    if (note.Type == NoteType.Unknow)
                    {
                        result.Errors.Add(
                            $"There is an unknown note type on beat {note.StartBeat} for player {note.PlayerNumber}");
                    }
                    else if (note.Type != NoteType.Eol)
                    {
                        if (!note.Duration.HasValue)
                        {
                            result.Errors.Add(
                                $"Note on beat {note.StartBeat} for player {note.PlayerNumber} has no duration");
                        }
                        else if (note.Duration.Value < 1)
                        {
                            result.Errors.Add(
                                $"Note on beat {note.StartBeat} for player {note.PlayerNumber} has a duration less than 1");
                        }

                        if (!note.Pitch.HasValue)
                        {
                            result.Errors.Add(
                                $"Note on beat {note.StartBeat} for player {note.PlayerNumber} has no pitch");
                        }

                        if (string.IsNullOrEmpty(note.Text))
                        {
                            result.Errors.Add(
                                $"Note on beat {note.StartBeat} for player {note.PlayerNumber} has no text (nor whitespace)");
                        }
                    }
                }

                if (!hasNotes)
                {
                    result.Errors.Add("The song contains no notes");
                }

                // No player info or only standard player one
                if (processedPlayerBeats.Count == 0 || processedPlayerBeats.Keys.SequenceEqual(new[] { 1 }))
                {
                    return result;
                }

                var songPlayers = song.GetPlayers();
                foreach (var playerNumber in processedPlayerBeats.Keys)
                {
                    if (!songPlayers.ContainsKey(playerNumber))
                    {
                        result.Errors.Add(
                            $"There is notes for player {playerNumber} defined but no corresponding player name using #P{playerNumber} header");
                    }
                }

                return result;
            }, cancellationToken);
        }
    }
}