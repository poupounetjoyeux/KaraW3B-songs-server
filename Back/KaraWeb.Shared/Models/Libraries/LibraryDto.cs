using System;

namespace KaraWeb.Shared.Models.Libraries
{
    /// <summary>
    ///     A songs library pointing to a folder containing songs
    /// </summary>
    public sealed class LibraryDto : IAnalyzableLibrary
    {
        /// <summary>
        ///     The library's ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     The library's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The library's description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     The library's path to directory
        /// </summary>
        public string Path { get; set; }
    }
}