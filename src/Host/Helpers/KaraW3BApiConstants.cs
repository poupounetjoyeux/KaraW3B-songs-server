using System.IO;

namespace KaraWeb.Server.Host.Helpers
{
    public static class KaraW3BApiConstants
    {
        public const string ApiMainRoutePrefix = "api";
        public const int Port = 7373;
        public static readonly string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "Config");
        public const string Log4NetConfigFile = "config.log4net.xml";
        public static readonly string Log4NetConfigPath = Path.Combine(ConfigPath, Log4NetConfigFile);
        public static readonly string Uri = "http://0.0.0.0:" + Port;
    }
}