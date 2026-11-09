using System.ComponentModel.DataAnnotations;

namespace Raceday.API.DTOs
{
    public class UpdateParticipantDto
    {
        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EmergencyContact { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? MedicalConditions { get; set; }
    }
}