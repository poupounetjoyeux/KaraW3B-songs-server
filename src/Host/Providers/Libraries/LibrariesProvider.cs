using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.SDK.Models.Libraries;
using KaraW3B.Server.Core.Jobs;
using KaraW3B.Server.Core.Persistence;
using KaraW3B.Server.Core.Persistence.Models.Libraries;
using KaraW3B.Server.Core.Services.FFmpeg;
using KaraW3B.Server.Core.Services.Scheduler;
using KaraW3B.Server.Core.Services.SongParser;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace KaraW3B.Server.Host.Providers.Libraries
{
    internal sealed class LibrariesProvider : ILibrariesProvider
    {
        private readonly KaraW3BDbContext _dbContext;
        private readonly ISongParserService _songParserService;
        private readonly ISchedulerService _schedulerService;
        private readonly IFFmpegService _ffmpegService;

        public LibrariesProvider(KaraW3BDbContext dbContext,
            ISongParserService songParserService, ISchedulerService schedulerService, IFFmpegService ffmpegService)
        {
            _dbContext = dbContext;
            _songParserService = songParserService;
            _schedulerService = schedulerService;
            _ffmpegService = ffmpegService;
            ReinitAnalyzingFlags();
        }

        private void ReinitAnalyzingFlags()
        {
            foreach (var library in _dbContext.Libraries)
            {
                if (library.IsAnalyzing)
                {
                    library.IsAnalyzing = false;
                }
            }

            _dbContext.SaveChanges();
        }

        public async IAsyncEnumerable<LibraryDto> GetLibrariesAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var library in _dbContext.Libraries.ToAsyncEnumerable().WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return library.ToDto();
            }
        }

        public Task<Library> GetLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
        {
            return _dbContext.Libraries.SingleOrDefaultAsync(c => c.Id == libraryId, cancellationToken);
        }

        public async Task<LibraryDto> CreateLibraryAsync(LibraryCreationPayload payload,
            CancellationToken cancellationToken)
        {
            var newLibrary = new Library
            {
                Name = payload.Name,
                Description = payload.Description,
                Path = payload.Path
            };
            var libraryEntry = await _dbContext.Libraries.AddAsync(newLibrary, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return libraryEntry.Entity.ToDto();
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

        public Task StartLibraryAnalyzeAsync(Library library, LibraryAnalyzeType analyzeType,
            CancellationToken cancellationToken)
        {
            var dataMap = new JobDataMap
            {
                [AnalyzeLibraryJob.LibraryKey] = library,
                [AnalyzeLibraryJob.AnalyzeTypeKey] = analyzeType,
                [AnalyzeLibraryJob.SongParserServiceKey] = _songParserService,
                [AnalyzeLibraryJob.FFmpegServiceKey] = _ffmpegService
            };
            return _schedulerService.StartJob(AnalyzeLibraryJob.JobKey, dataMap, cancellationToken);
        }
    }
}