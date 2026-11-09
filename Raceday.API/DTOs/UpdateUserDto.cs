using System.ComponentModel.DataAnnotations;

namespace Raceday.API.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}