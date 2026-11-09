
using System.ComponentModel.DataAnnotations;

namespace Raceday.API.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"

        // ----- Organiser-only fields (optional) -----
        [MaxLength(100)]
        public string? CompanyName { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        // ----- Participant-only fields (optional) -----
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(100)]
        public string? EmergencyContact { get; set; }

        [MaxLength(500)]
        public string? MedicalConditions { get; set; }
    }
}
