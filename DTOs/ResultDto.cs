using System.ComponentModel.DataAnnotations;

namespace Raceday.API.DTOs
{
    public class ResultDto
    {
        [Required]
        public int EnrolmentID { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        [Range(1, 100000)]
        public int Position { get; set; }

        public bool IsDisqualified { get; set; } = false;

        [MaxLength(255)]
        public string? Notes { get; set; }
    }
}