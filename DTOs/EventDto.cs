using System.ComponentModel.DataAnnotations;

namespace Raceday.API.DTOs
{
    public class EventDto
    {
        [Required]
        [MaxLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDateAndTime { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 9999.99)]
        public decimal Distance { get; set; }

        [Required]
        [MaxLength(30)]
        public string EventType { get; set; } = string.Empty; // "Road Running", "Walking", "Cycling"

        [MaxLength(20)]
        public string Status { get; set; } = "Open"; // "Open", "Closed", "Cancelled"

        [Required]
        [Range(1, 100000)]
        public int MaxParticipants { get; set; }
    }
}