using System;

namespace KaraW3B.Server.Songs.Core.Models.Exceptions
{
    public class KaraW3BSongsServerException : Exception
    {
        public KaraW3BSongsServerException(string message) : base(message)
        {
        }
    }
}