using System.Threading;
using System.Threading.Tasks;
using KaraWeb.Shared.Models.Libraries;

namespace KaraWeb.Core.Services.LibrariesAnalyzer
{
    public interface ILibrariesAnalyzerService
    {
        Task StartLibraryAnalyzeAsync(IAnalyzableLibrary library, LibraryAnalyzeType analyzeType,
            CancellationToken cancellationToken);
    }
}