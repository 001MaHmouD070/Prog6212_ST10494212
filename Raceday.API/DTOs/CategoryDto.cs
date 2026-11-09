using System.ComponentModel.DataAnnotations;

namespace Raceday.API.DTOs
{
    public class CategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Range(0, 120)]
        public int MinAge { get; set; }

        [Required]
        [Range(0, 120)]
        public int MaxAge { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [Range(0, 99999.99)]
        public decimal EntryFee { get; set; }
    }
}