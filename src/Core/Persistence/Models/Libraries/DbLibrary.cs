using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Songs.Models.Libraries;
using Microsoft.EntityFrameworkCore;

namespace KaraW3B.Server.Songs.Core.Persistence.Models.Libraries
{
    [Table("Libraries")]
    public class DbLibrary
    {
        public static readonly LibraryAnalyzeStatus[] AnalyzingStatus = {
            LibraryAnalyzeStatus.Pending,
            LibraryAnalyzeStatus.Analyzing
        };

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public LibraryAnalyzeStatus AnalyzeStatus { get; set; } = LibraryAnalyzeStatus.Never;

        public string LastAnalyzeMessage { get; set; }

        [NotMapped]
        public bool CanStartAnalyze => !AnalyzingStatus.Contains(AnalyzeStatus);

        public static async Task<bool> TryMarkAsPendingAsync(KaraW3BDbContext dbContext, Guid libraryId,
            CancellationToken cancellationToken)
        {
            var updatedRows = await dbContext.Libraries
                .Where(l => l.Id == libraryId && l.CanStartAnalyze)
                .ExecuteUpdateAsync(
                s => s
                    .SetProperty(l => l.AnalyzeStatus, LibraryAnalyzeStatus.Pending)
                    .SetProperty(l => l.LastAnalyzeMessage, $"Analyze queued at {DateTime.Now:dd/MM/yyyy hh:mm:ss}"),
                cancellationToken: cancellationToken);
            return updatedRows > 0;
        }

        public static async Task<bool> TryMarkAsAnalyzingAsync(KaraW3BDbContext dbContext, Guid libraryId,
            CancellationToken cancellationToken)
        {
            var updatedRows = await dbContext.Libraries
                .Where(l => l.Id == libraryId && l.AnalyzeStatus == LibraryAnalyzeStatus.Pending)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(l => l.AnalyzeStatus, LibraryAnalyzeStatus.Analyzing)
                        .SetProperty(l => l.LastAnalyzeMessage,
                            $"Analyze started at {DateTime.Now:dd/MM/yyyy hh:mm:ss}"),
                    cancellationToken: cancellationToken);

            return updatedRows > 0;
        }

        public static async Task MarkAs(KaraW3BDbContext dbContext, Guid libraryId, bool isSuccess,
            string message, CancellationToken cancellationToken)
        {
            await dbContext.Libraries
                .Where(l => l.Id == libraryId)
                .ExecuteUpdateAsync(s => s.
                    SetProperty(l => l.AnalyzeStatus,
                        isSuccess ? LibraryAnalyzeStatus.Success : LibraryAnalyzeStatus.Error)
                    .SetProperty(l => l.LastAnalyzeMessage, message),
                cancellationToken: cancellationToken);
        }

        public Library ToLibrary()
        {
            return new Library
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Path = Path,
                AnalyzeStatus = AnalyzeStatus,
                LastAnalyzeMessage = LastAnalyzeMessage
            };
        }
    }
}