using System.Text.Json;
using KaraW3B.SDK.Helpers;
using KaraW3B.Server.Core.Persistence.Models.Songs;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KaraW3B.Server.Core.Persistence.Converters
{
    internal sealed class SongMedleyValueConverter : ValueConverter<SongMedley, string>
    {
        public SongMedleyValueConverter() : base(
            s => s == null ? null : JsonSerializer.Serialize(s, JsonHelper.DefaultJsonSerializerOptions),
            s => string.IsNullOrEmpty(s)
                ? null
                : JsonSerializer.Deserialize<SongMedley>(s, JsonHelper.DefaultJsonSerializerOptions))
        {
        }
    }
}