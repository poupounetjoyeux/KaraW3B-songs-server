using System.Collections.Generic;

namespace KaraWeb.Shared.Models.Songs
{
    public interface IAnalyzableSong
    {
        string Encoding { get; }
        string Title { get; }
        string Artist { get; }
        string Audio { get; }
        string Video { get; }
        string Cover { get; }
        string Background { get; }
        double? Bpm { get; }
        Dictionary<int, string> GetPlayers();
    }
}