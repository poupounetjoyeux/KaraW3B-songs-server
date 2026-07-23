using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Interpreters;
using KaraW3B.Interpreters.Helpers;
using KaraW3B.Interpreters.Models;
using KaraW3B.Server.Songs.Core.Persistence.Models.Songs;
using KaraW3B.Server.Songs.Core.Services.Settings;
using KaraW3B.Server.Songs.Models.Songs.Alerts;
using log4net;
using Nito.AsyncEx;

namespace KaraW3B.Server.Songs.Core.Services.SongFileInterpreter
{
    public sealed class SongFileInterpreterService : ISongFileInterpreterService
    {
        private readonly ISettingsService _settingsService;
        private readonly ConcurrentDictionary<string, AsyncReaderWriterLock> _fileLockers = new();
        private readonly ILog _logger = LogManager.GetLogger(nameof(SongFileInterpreterService));

        public SongFileInterpreterService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private AwaitableDisposable<IDisposable> GetLock(string filePath, bool write)
        {
            var locker = _fileLockers.GetOrAdd(filePath, _ => new AsyncReaderWriterLock());
            return write ? locker.WriterLockAsync() : locker.ReaderLockAsync();
        }

        public async Task<bool> ParseSongAndCheckAsync(FileInfo songFile, DbSong song,
            CancellationToken cancellationToken)
        {
            if (!songFile.Exists)
            {
                _logger.Error($"The song file '{songFile.FullName}' was not found");
                return false;
            }

            var songProxy = new InterpretableSongProxy(song);
            _logger.Info($"Start parsing song file '{songFile.FullName}'");
            
            var timeWatch = new Stopwatch();
            timeWatch.Start();

            using var locker = await GetLock(songFile.FullName, false);
            var parseResult = await SongParser.ParseSongAsync(songFile, songProxy, cancellationToken);
            AddAlerts(parseResult, song);

            timeWatch.Stop();
            _logger.Info(
                $"Song file '{songFile.FullName}' successfully parsed in {timeWatch.Elapsed}");

            song.LastParseTime = DateTime.Now;

            await AnalyzeSongErrors(song, cancellationToken);
            return true;
        }

        private static void AddAlerts(InterpreterResult result, DbSong song)
        {
            song.Alerts.AddRange(result.Fatals.Select(a => new DbSongAlert
                { Type = AlertType.Parsing, Level = AlertLevel.Fatal, Message = a.Message, FileLine = a.FileLine }));

            song.Alerts.AddRange(result.Errors.Select(a => new DbSongAlert
                { Type = AlertType.Parsing, Level = AlertLevel.Error, Message = a.Message, FileLine = a.FileLine }));

            song.Alerts.AddRange(result.Warnings.Select(a => new DbSongAlert
                { Type = AlertType.Parsing, Level = AlertLevel.Warning, Message = a.Message, FileLine = a.FileLine }));
        }

        public async Task WriteSongFile(DbSong songToWrite, string filePath, bool overwrite, CancellationToken cancellationToken)
        {
            var songProxy = new InterpretableSongProxy(songToWrite);

            _logger.Info($"Start writing song file '{filePath}'");

            var timeWatch = new Stopwatch();
            timeWatch.Start();

            using var locker = await GetLock(filePath, true);
            songToWrite.Version = _settingsService.Settings.FileVersionToWrite;
            await SongWriter.WriteSongAsync(songProxy, filePath, overwrite, cancellationToken);

            timeWatch.Stop();
            _logger.Info(
                $"Song file '{filePath}' successfully wrote in {timeWatch.Elapsed}");
        }

        private static async Task AnalyzeSongErrors(DbSong song, CancellationToken cancellationToken)
        {
            var analyzeResult = await SongValidationHelper.CheckFullSongErrorsAsync(song, song.Notes, cancellationToken);
            foreach (var infoError in analyzeResult.InfoErrors)
            {
                song.Alerts.Add(new DbSongAlert
                {
                    Type = AlertType.Info,
                    Level = infoError.IsWarning ? AlertLevel.Warning : AlertLevel.Error,
                    Message = infoError.Message
                });
            }

            foreach (var noteError in analyzeResult.NotesErrors)
            {
                song.Alerts.Add(new DbSongAlert
                {
                    Type = AlertType.Note,
                    Level = AlertLevel.Error,
                    Message = noteError.Message,
                    FileLine = noteError.FileLine
                });
            }
        }
    }
}