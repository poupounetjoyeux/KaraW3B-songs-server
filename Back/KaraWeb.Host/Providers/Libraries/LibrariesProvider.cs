using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KaraWeb.Core.Persistence;
using KaraWeb.Core.Persistence.Libraries;
using KaraWeb.Core.Services.LibrariesAnalyzer;
using KaraWeb.Shared.Models.Libraries;
using Microsoft.EntityFrameworkCore;

namespace KaraWeb.Host.Providers.Libraries
{
    internal sealed class LibrariesProvider : ILibrariesProvider
    {
        private readonly KaraWebDbContext _dbContext;
        private readonly ILibrariesAnalyzerService _librariesAnalyzerService;

        public LibrariesProvider(KaraWebDbContext dbContext, ILibrariesAnalyzerService librariesAnalyzerService)
        {
            _dbContext = dbContext;
            _librariesAnalyzerService = librariesAnalyzerService;
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

        public async Task<LibraryDto> GetLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
        {
            return (await _dbContext.Libraries.SingleOrDefaultAsync(c => c.Id == libraryId, cancellationToken)).ToDto();
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

        public Task StartLibraryAnalyzeAsync(IAnalyzableLibrary library, LibraryAnalyzeType libraryAnalyzeType,
            CancellationToken cancellationToken)
        {
            return _librariesAnalyzerService.StartLibraryAnalyzeAsync(library, libraryAnalyzeType, cancellationToken);
        }

        public async Task<bool> DeleteLibraryAsync(Guid libraryId, CancellationToken cancellationToken)
        {
            var deleteCount = await _dbContext.Songs.Where(s => s.LibraryId == libraryId).ExecuteDeleteAsync(cancellationToken);
            deleteCount += await _dbContext.Libraries.Where(l => l.Id == libraryId).ExecuteDeleteAsync(cancellationToken: cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return deleteCount > 0;
        }
    }
}