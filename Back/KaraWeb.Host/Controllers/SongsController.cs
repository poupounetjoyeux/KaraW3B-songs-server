using KaraWeb.Host.Providers.Songs;
using KaraWeb.Shared.Models.Songs;
using KaraWeb.Shared.Models.Songs.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KaraWeb.Host.Controllers
{
    [Route(Name)]
    public sealed class SongsController : ControllerBase
    {
        public const string Name = "songs";

        private readonly ISongsProvider _songsProvider;

        public SongsController(ISongsProvider songsProvider)
        {
            _songsProvider = songsProvider;
        }

        /// <summary>
        ///     Get song's details (containing notes, errors, warnings, etc...)  by its ID
        /// </summary>
        /// <param name="songId">The ID of the song to retrieve</param>
        /// <param name="cancellationToken"></param>
        [HttpGet("{songId}/details")]
        [SwaggerResponse(StatusCodes.Status200OK, "The asked song's details", typeof(DetailedSongDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No song found with the given ID", typeof(string))]
        public async Task<ActionResult<DetailedSongDto>> GetSongAsync([FromRoute] Guid songId,
            CancellationToken cancellationToken = default)
        {
            var song = await _songsProvider.GetDetailedSongAsync(songId, cancellationToken);
            if (song == null)
            {
                return NotFound($"The song with ID {songId} doesn't exist");
            }

            return Ok(song);
        }

        /// <summary>
        ///     Get song's file stream by its ID and the file stream type
        /// </summary>
        /// <param name="songId">The ID of the song to retrieve</param>
        /// <param name="fileType">The file type from the song you want to stream</param>
        /// <param name="cancellationToken"></param>
        [HttpGet("{songId}/streams/{fileType}")]
        [SwaggerResponse(StatusCodes.Status200OK, "The asked song's file stream", typeof(Stream))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No song found with the given ID", typeof(string))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The song doesn't have this file type", typeof(string))]
        public async Task<IActionResult> GetSongFileStream([FromRoute] Guid songId, [FromRoute] SongFileType fileType,
            CancellationToken cancellationToken = default)
        {
            var song = await _songsProvider.GetSongById(songId, cancellationToken);
            if (song == null)
            {
                return NotFound($"The song with ID {songId} doesn't exist");
            }

            var streamResult = await _songsProvider.GetSongFileStream(song, fileType, cancellationToken);
            if (streamResult == null)
            {
                return BadRequest($"The song {songId} has no file of type {fileType} or the file was not found on server");
            }

            return streamResult;
        }
    }
}