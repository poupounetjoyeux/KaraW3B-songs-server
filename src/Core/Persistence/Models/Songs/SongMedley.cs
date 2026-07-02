using System;
using KaraW3B.SDK.Models.Songs.Medleys;

namespace KaraW3B.Server.Core.Persistence.Models.Songs
{
    public class SongMedley : ISongMedley
    {
        public TimeSpan MedleyStart { get; set; }

        public TimeSpan MedleyEnd { get; set; }

        public SongMedleyDto ToDto()
        {
            return new SongMedleyDto
            {
                MedleyStart = MedleyStart,
                MedleyEnd = MedleyEnd
            };
        }
    }
}