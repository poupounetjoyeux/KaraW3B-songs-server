using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KaraW3B.SDK.Models.Songs;
using KaraW3B.SDK.Models.Songs.Alerts;
using KaraW3B.SDK.Models.Songs.Files;
using KaraW3B.SDK.Models.Songs.Notes;
using KaraW3B.Server.Host.Providers.Songs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace KaraW3B.Server.Host.Controllers
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
        ///     Get song by its ID
        /// </summary>
        /// <param name="songId">The ID of the song to retrieve</param>
        /// <param name="cancellationToken"></param>
        [HttpGet("{songId}")]
        [SwaggerResponse(StatusCodes.Status200OK, "The asked song's details", typeof(SongDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No song found with the given ID", typeof(string))]
        public async Task<ActionResult<SongDto>> GetSongAsync([FromRoute] Guid songId,
            CancellationToken cancellationToken = default)
        {
            var song = await _songsProvider.GetSongById(songId, cancellationToken);
            if (song == null)
            {
                return NotFound($"The song with ID {songId} doesn't exist");
            }

            return Ok(song.ToDto());
        }

        /// <summary>
        ///     Get song's notes by its ID
        /// </summary>
        /// <param name="songId">The ID of the song</param>
        /// <param name="cancellationToken"></param>
        [HttpGet("{songId}/notes")]
        [SwaggerResponse(StatusCodes.Status200OK, "The asked song's notes", typeof(List<SongNoteDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No song found with the given ID", typeof(string))]
        public async Task<ActionResult<List<SongNoteDto>>> GetSongNotesAsync([FromRoute] Guid songId,
            CancellationToken cancellationToken = default)
        {
            var song = await _songsProvider.GetSongById(songId, cancellationToken);
            if (song == null)
            {
                return NotFound($"The song with ID {songId} doesn't exist");
            }

            return Ok(song.Notes.Select(n => n.ToDto()).ToList());
        }

        /// <summary>
        ///     Get song's alerts by its ID
        /// </summary>
        /// <param name="songId">The ID of the song</param>
        /// <param name="cancellationToken"></param>
        [HttpGet("{songId}/alerts")]
        [SwaggerResponse(StatusCodes.Status200OK, "The asked song's alerts", typeof(List<SongAlertDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No song found with the given ID", typeof(string))]
        public async Task<ActionResult<List<SongNoteDto>>> GetSongAlertsAsync([FromRoute] Guid songId,
            CancellationToken cancellationToken = default)
        {
            var song = await _songsProvider.GetSongById(songId, cancellationToken);
            if (song == null)
            {
                return NotFound($"The song with ID {songId} doesn't exist");
            }

            return Ok(song.Alerts.Select(a => a.ToDto()).ToList());
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
        public async Task<IActionResult> GetSongFileStream([FromRoute] Guid songId, [FromRoute] FileType fileType,
            CancellationToken cancellationToken = default)
        {
            var song = await _songsProvider.GetSongById(songId, cancellationToken);
            if (song == null)
            {
                return NotFound($"The song with ID {songId} doesn't exist");
            }

            if (!song.SongFileExist(fileType))
            {
                return NotFound($"The {fileType} file doesn't exist for song with ID {songId}");
            }

            var streamResult = await _songsProvider.GetSongFileStream(song, fileType, cancellationToken);
            if (streamResult == null)
            {
                return BadRequest(
                    $"The song {songId} has no file of type {fileType}");
            }

            return streamResult;
        }
    }
}