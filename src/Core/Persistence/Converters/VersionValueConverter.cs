using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KaraW3B.Server.Core.Persistence.Converters
{
    internal sealed class VersionValueConverter : ValueConverter<Version, string>
    {
        public VersionValueConverter() : base(
            v => v == null ? null : v.ToString(3),
            v => string.IsNullOrEmpty(v) ? null : Version.Parse(v))
        {
        }
    }
}