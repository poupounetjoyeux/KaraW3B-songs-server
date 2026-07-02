using System.IO;
using KaraW3B.SDK;

namespace KaraW3B.Server.Core
{
    public sealed class KaraW3BFileHelper : IFileHelper
    {
        public bool IsRelativePath(string path)
        {
            return !Path.IsPathFullyQualified(path);
        }
    }
}