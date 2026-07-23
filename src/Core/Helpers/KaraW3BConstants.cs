using System.Reflection;

namespace KaraW3B.Server.Songs.Core.Helpers
{
    public static class KaraW3BConstants
    {
        public const string ApplicationName = "KaraW3B-Songs-Server";

        public static readonly string ApplicationVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
    }
}