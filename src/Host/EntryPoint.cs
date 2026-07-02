using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using KaraW3B.SDK.Helpers;
using KaraW3B.Server.Core.Persistence;
using KaraWeb.Server.Host.Helpers;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KaraWeb.Server.Host
{
    internal sealed class EntryPoint
    {
        public static async Task Main(string[] args)
        {
            var logger = ConfigureLog4NetAndGetLogger();
            if (!await KaraW3BDbContext.EnsureDatabase(logger))
            {
                return;
            }

            var server = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<Startup>()
                        .UseUrls(KaraW3BApiConstants.Uri)
                        .ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
                });
            logger.Info($"Server is now started and listen at: {KaraW3BApiConstants.Uri}");
            await server.RunConsoleAsync();
        }

        private static ILog ConfigureLog4NetAndGetLogger()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(KaraW3BApiConstants.Log4NetConfigPath));
            return LogManager.GetLogger(KaraW3BConstants.ApplicationName);
        }
    }
}