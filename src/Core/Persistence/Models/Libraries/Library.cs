using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KaraW3B.SDK.Models.Libraries;

namespace KaraW3B.Server.Core.Persistence.Models.Libraries
{
    [Table("Libraries")]
    public class Library
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public bool IsAnalyzing { get; set; }

        public string LastAnalyzeMessage { get; set; }

        public LibraryDto ToDto()
        {
            return new LibraryDto
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Path = Path,
                IsAnalyzing = IsAnalyzing,
                LastAnalyzeMessage = LastAnalyzeMessage
            };
        }
    }
}