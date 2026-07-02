using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KaraW3B.SDK.Models.Songs.Players;
using Microsoft.EntityFrameworkCore;

namespace KaraW3B.Server.Core.Persistence.Models.Songs
{
    [Table("SongPlayers")]
    [PrimaryKey(nameof(SongId), nameof(Number))]
    public class SongPlayer
    {
        public Guid SongId { get; set; }

        [ForeignKey(nameof(SongId))]
        public virtual Song Song { get; set; }

        [Required]
        public int Number { get; set; }

        [MaxLength(200)]
        [Required]
        public string Name { get; set; }

        public SongPlayerDto ToDto()
        {
            return new SongPlayerDto
            {
                Name = Name,
                PlayerNumber = Number
            };
        }
    }
}