using System;

namespace KaraWeb.Shared.Models.Libraries
{
    public interface IAnalyzableLibrary
    {
        Guid Id { get; }
        string Name { get; }
        string Path { get; }
    }
}