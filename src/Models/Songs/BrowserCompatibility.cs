namespace KaraW3B.Server.Songs.Models.Songs
{
    /// <summary>
    ///     The set of available file web browsers compatibility statuses
    /// </summary>
    public enum BrowserCompatibility
    {
        /// <summary>
        ///     The file compatibility was not yet checked
        /// </summary>
        NotChecked,
        /// <summary>
        ///     A conversion to format MP4 H.264 is mandatory to make this file working in web browsers
        /// </summary>
        ConversionMandatory,
        /// <summary>
        ///     A conversion to format MP4 H.264 is recommended to have better performances and compatibility in web browsers
        /// </summary>
        ConversionRecommended,
        /// <summary>
        ///     The file is compatible with web browsers
        /// </summary>
        Compatible
    }
}
