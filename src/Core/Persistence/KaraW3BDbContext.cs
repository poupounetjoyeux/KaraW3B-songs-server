using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.Server.Songs.Core.Persistence.Converters;
using KaraW3B.Server.Songs.Core.Persistence.Models.Libraries;
using KaraW3B.Server.Songs.Core.Persistence.Models.Songs;
using KaraW3B.Server.Songs.Models.Libraries;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace KaraW3B.Server.Songs.Core.Persistence
{
    public sealed class KaraW3BDbContext : DbContext
    {
        private const string DbFileName = "KaraW3B.db";
        private static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
        private static readonly string DbFilePath = Path.Combine(DataPath, DbFileName);

        public DbSet<DbLibrary> Libraries { get; set; }

        public DbSet<DbSong> Songs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite($"Data Source={DbFilePath}").UseLazyLoadingProxies();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<TimeSpan?>()
                .HaveConversion<TimeSpanValueConverter>();

            configurationBuilder
                .Properties<TimeSpan>()
                .HaveConversion<TimeSpanValueConverter>();

            configurationBuilder
                .Properties<Version>()
                .HaveConversion<VersionValueConverter>();

            configurationBuilder.Properties<DbSongMedley>().HaveConversion<SongMedleyValueConverter>();
        }

        public static async Task<bool> EnsureDatabase(ILog logger, CancellationToken cancellationToken)
        {
            try
            {
                if (!Directory.Exists(DataPath))
                {
                    Directory.CreateDirectory(DataPath);
                    logger.Info("Created data directory");
                }

                await using var context = new KaraW3BDbContext();
                await context.Database.MigrateAsync(cancellationToken: cancellationToken);
                logger.Info("Database initialized successfully");

                await ReinitFlagsAfterPanicShutdown(context, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                logger.Fatal($"Error when initializing the database: {ex}");
                return false;
            }
        }

        private static async Task ReinitFlagsAfterPanicShutdown(KaraW3BDbContext dbContext, CancellationToken cancellationToken)
        {
            await dbContext.Libraries
                .Where(l => !l.CanStartAnalyze)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(l => l.AnalyzeStatus, LibraryAnalyzeStatus.Error)
                    .SetProperty(l => l.LastAnalyzeMessage, "The analyze was interrupted"),
                cancellationToken: cancellationToken);
        }
    }
}