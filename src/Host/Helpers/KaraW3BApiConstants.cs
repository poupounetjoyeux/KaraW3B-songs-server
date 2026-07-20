using System.IO;

namespace KaraW3B.Server.Songs.Host.Helpers
{
    public static class KaraW3BApiConstants
    {
        public const string ApiMainRoutePrefix = "api";
        public const int Port = 7373;
        public static readonly string ConfigDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "config");
        public const string Log4NetConfigFile = "config.log4net.xml";
        public static readonly string DefaultLog4NetConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "log4net", Log4NetConfigFile);
        public static readonly string Log4NetConfigPath = Path.Combine(ConfigDirectoryPath, Log4NetConfigFile);
        public const string ConfigFile = "config.json";
        public static readonly string ConfigFilePath = Path.Combine(ConfigDirectoryPath, ConfigFile);
        public static readonly string Uri = "http://0.0.0.0:" + Port;
    }
}