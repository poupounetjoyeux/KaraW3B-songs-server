using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KaraWeb.Shared.Models.Libraries;

namespace KaraWeb.Core.Persistence.Libraries
{
    [Table("Libraries")]
    public sealed class Library : IAnalyzableLibrary
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Path { get; set; }

        public LibraryDto ToDto()
        {
            return new LibraryDto
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Path = Path
            };
        }
    }
}