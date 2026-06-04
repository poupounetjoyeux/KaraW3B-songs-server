using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KaraWeb.Shared.Models.Libraries;

namespace KaraWeb.Host.Providers.Libraries
{
    public interface ILibrariesProvider
    {
        IAsyncEnumerable<LibraryDto> GetLibrariesAsync(CancellationToken cancellationToken);
        Task<LibraryDto> GetLibraryAsync(Guid libraryId, CancellationToken cancellationToken);
        Task<LibraryDto> CreateLibraryAsync(LibraryCreationPayload payload, CancellationToken cancellationToken);
        Task<bool> DeleteLibraryAsync(Guid libraryId, CancellationToken cancellationToken);
        Task StartLibraryAnalyzeAsync(IAnalyzableLibrary library, LibraryAnalyzeType analyzeType,
            CancellationToken cancellationToken);
    }
}