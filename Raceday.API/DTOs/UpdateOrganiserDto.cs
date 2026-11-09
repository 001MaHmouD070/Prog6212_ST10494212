using System.ComponentModel.DataAnnotations;

namespace Raceday.API.DTOs
{
    public class UpdateOrganiserDto
    {
        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}