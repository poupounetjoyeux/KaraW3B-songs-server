using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Songs.Core.Jobs;
using KaraW3B.Server.Songs.Core.Persistence;
using KaraW3B.Server.Songs.Core.Persistence.Models.Libraries;
using KaraW3B.Server.Songs.Core.Services.FFmpeg;
using KaraW3B.Server.Songs.Core.Services.Scheduler;
using KaraW3B.Server.Songs.Core.Services.Settings;
using KaraW3B.Server.Songs.Core.Services.SongFileInterpreter;
using KaraW3B.Server.Songs.Models.Libraries;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace KaraW3B.Server.Songs.Host.Providers.Libraries
{
    internal sealed class LibrariesProvider : ILibrariesProvider
    {
        private readonly KaraW3BDbContext _dbContext;
        private readonly ISongFileInterpreterService _songFileInterpreterService;
        private readonly IFFmpegService _ffmpegService;
        private readonly ApiScheduler _scheduler;

        public LibrariesProvider(ISettingsService settingsService, KaraW3BDbContext dbContext,
            ISongFileInterpreterService songFileInterpreterService, ISchedulerService schedulerService, IFFmpegService ffmpegService)
        {
            _dbContext = dbContext;
            _songFileInterpreterService = songFileInterpreterService;
            _ffmpegService = ffmpegService;

            _scheduler = schedulerService.RegisterSchedulerAsync("Libraries",
                    settingsService.Settings.ConcurrencySettings.MaxLibraryAnalyzesConcurrency, CancellationToken.None)
                .Result;
            _scheduler.RegisterJobAsync<AnalyzeLibraryJob>(AnalyzeLibraryJob.JobKey, CancellationToken.None).Wait();
        }

        public async IAsyncEnumerable<Library> GetLibrariesAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var library in _dbContext.Libraries.ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return library.ToLibrary();
            }
        }

        public Task<DbLibrary> GetLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
        {
            return _dbContext.Libraries.SingleOrDefaultAsync(c => c.Id == libraryId, cancellationToken);
        }

        public async Task<Library> CreateLibraryAsync(LibraryCreationPayload payload,
            CancellationToken cancellationToken)
        {
            var newLibrary = new DbLibrary
            {
                Name = payload.Name,
                Description = payload.Description,
                Path = payload.Path
            };
            var libraryEntry = await _dbContext.Libraries.AddAsync(newLibrary, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return libraryEntry.Entity.ToLibrary();
        }

        public async Task<bool> DeleteLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
        {
            var deleteCount = await _dbContext.Songs.Where(s => s.LibraryId == libraryId)
                .ExecuteDeleteAsync(cancellationToken);
            deleteCount += await _dbContext.Libraries.Where(l => l.Id == libraryId)
                .ExecuteDeleteAsync(cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return deleteCount > 0;
        }

        public async Task StartLibraryAnalyzeAsync(DbLibrary library, LibraryAnalyzeType analyzeType,
            CancellationToken cancellationToken)
        {
            var dataMap = new JobDataMap
            {
                [AnalyzeLibraryJob.LibraryIdKey] = library.Id,
                [AnalyzeLibraryJob.LibraryPathKey] = library.Path,
                [AnalyzeLibraryJob.AnalyzeTypeKey] = analyzeType,
                [AnalyzeLibraryJob.SongParserServiceKey] = _songFileInterpreterService,
                [AnalyzeLibraryJob.FFmpegServiceKey] = _ffmpegService
            };

            if (!await DbLibrary.TryMarkAsPendingAsync(_dbContext, library.Id, cancellationToken))
            {
                return;
            }

            await _scheduler.StartJob(AnalyzeLibraryJob.JobKey, dataMap, cancellationToken);
        }
    }
}