using KaraWeb.Core.Models.Songs;
using KaraWeb.Core.Models.Songs.Notes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace KaraWeb.Core.Helpers
{
    public static class SongHelper
    {
        public static async Task<string> ComputeFileHash(FileInfo file, CancellationToken cancellationToken)
        {
            var fileBytes = await File.ReadAllBytesAsync(file.FullName, cancellationToken);
            var hashBytes = SHA1.HashData(fileBytes);
            return Convert.ToHexStringLower(hashBytes);
        }

        public static Task CheckHeadersErrorsAsync(Song song, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                if (!song.CollectionId.HasValue)
                {
                    song.Warnings.Add("The song is orphan and didn't have collection reference");
                }

                if (string.IsNullOrEmpty(song.Title))
                {
                    song.Errors.Add("A title is mandatory (#TITLE header)");
                }

                if (string.IsNullOrEmpty(song.Artist))
                {
                    song.Errors.Add("An artist is mandatory (#ARTIST header)");
                }

                if (!song.Bpm.HasValue)
                {
                    song.Errors.Add("The song BPM is mandatory (#BPM header)");
                }
                else if (song.Bpm.Value < 1)
                {
                    song.Errors.Add("The song BPM is less than 1");
                }

                if (string.IsNullOrEmpty(song.Audio))
                {
                    song.Errors.Add("The song audio file is mandatory (#AUDIO header)");
                } 
                else if (Path.IsPathFullyQualified(song.Audio))
                {
                    song.Errors.Add("The song audio file must be a relative path");
                }

                if (!string.IsNullOrEmpty(song.Video) && Path.IsPathFullyQualified(song.Video))
                {
                    song.Errors.Add("The song video file must be a relative path");
                }

                if (!string.IsNullOrEmpty(song.Cover) && Path.IsPathFullyQualified(song.Cover))
                {
                    song.Errors.Add("The song cover file must be a relative path");
                }

                if (!string.IsNullOrEmpty(song.Background) && Path.IsPathFullyQualified(song.Background))
                {
                    song.Errors.Add("The song background file must be a relative path");
                }

                if (song.NotManagedHeaders.Any())
                {
                    song.Warnings.Add($"There is {song.NotManagedHeaders.Count} unmanaged headers");
                }

                foreach (var songPlayer in song.Players)
                {
                    if (string.IsNullOrEmpty(songPlayer.Name))
                    {
                        song.Errors.Add(
                            $"The player {songPlayer.Number} has no name defined (#P{songPlayer.Number} header)");
                    }
                }

                if (!song.HasEofMarker)
                {
                    song.Warnings.Add("The song doesn't contains the 'E' EOF marker");
                }
            }, cancellationToken);
        }

        public static Task CheckNotesErrorsAsync(Song song, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var processedPlayerBeats = new Dictionary<int, HashSet<int>>();
                foreach (var note in song.Notes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (note.StartBeat < 1)
                    {
                        song.Errors.Add(
                            $"Note {note.StartBeat} for player {note.PlayerNumber} must start at least at beat 1");
                    }

                    if (!processedPlayerBeats.TryGetValue(note.PlayerNumber, out var playerProcessedBeats))
                    {
                        playerProcessedBeats = new HashSet<int>(note.StartBeat);
                        processedPlayerBeats.Add(note.PlayerNumber, playerProcessedBeats);
                    }
                    else if (playerProcessedBeats.Contains(note.StartBeat))
                    {
                        song.Errors.Add(
                            $"There is a duplicated note on beat {note.StartBeat} for player {note.PlayerNumber}");
                    }

                    if (note.Type == NoteType.Unknow)
                    {
                        song.Errors.Add(
                            $"There is an unknown note type on beat {note.StartBeat} for player {note.PlayerNumber}");
                    }
                    else if (note.Type != NoteType.Eol)
                    {
                        if (!note.Duration.HasValue)
                        {
                            song.Errors.Add(
                                $"Note on beat {note.StartBeat} for player {note.PlayerNumber} has no duration");
                        } 
                        else if (note.Duration.Value < 1)
                        {
                            song.Errors.Add(
                                $"Note on beat {note.StartBeat} for player {note.PlayerNumber} has a duration less than 1");
                        }

                        if (!note.Pitch.HasValue)
                        {
                            song.Errors.Add(
                                $"Note on beat {note.StartBeat} for player {note.PlayerNumber} has no pitch");
                        }

                        if (string.IsNullOrEmpty(note.Text))
                        {
                            song.Errors.Add(
                                $"Note on beat {note.StartBeat} for player {note.PlayerNumber} has no text (nor whitespace)");
                        }
                    }
                }

                // No player info or only standard player one
                if (processedPlayerBeats.Count == 0 || processedPlayerBeats.Keys.SequenceEqual(new[] { 1 }))
                {
                    return;
                }

                foreach (var playerNumber in processedPlayerBeats.Keys)
                {
                    if (song.Players.All(p => p.Number != playerNumber))
                    {
                        song.Errors.Add(
                            $"There is notes for player {playerNumber} defined but no corresponding player name using #P{playerNumber} header");
                    }
                }
            }, cancellationToken);
        }
    }
}
