using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KaraW3B.SDK.Models.Songs.Alerts;
using Microsoft.EntityFrameworkCore;

namespace KaraW3B.Server.Core.Persistence.Models.Songs
{
    [Table("SongAlerts")]
    [Index(nameof(SongId))]
    public class SongAlert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid SongId { get; set; }

        [ForeignKey(nameof(SongId))]
        public virtual Song Song { get; set; }

        [Required]
        public AlertType Type { get; set; }

        [Required]
        public AlertLevel Level { get; set; }

        [Required]
        public string Message { get; set; }

        public int? FileLine { get; set; }

        public SongAlertDto ToDto()
        {
            return new SongAlertDto
            {
                Type = Type,
                Level = Level,
                Message = Message,
                FileLine = FileLine
            };
        }
    }
}