using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.SDK.Models.Libraries;
using KaraW3B.Server.Core.Persistence.Models.Libraries;

namespace KaraWeb.Server.Host.Providers.Libraries
{
    public interface ILibrariesProvider
    {
        IAsyncEnumerable<LibraryDto> GetLibrariesAsync(CancellationToken cancellationToken);
        Task<Library> GetLibraryAsync(Guid libraryId, CancellationToken cancellationToken);
        Task<LibraryDto> CreateLibraryAsync(LibraryCreationPayload payload, CancellationToken cancellationToken);
        Task<bool> DeleteLibraryAsync(Guid libraryId, CancellationToken cancellationToken);

        Task StartLibraryAnalyzeAsync(Library library, LibraryAnalyzeType analyzeType,
            CancellationToken cancellationToken);
    }
}