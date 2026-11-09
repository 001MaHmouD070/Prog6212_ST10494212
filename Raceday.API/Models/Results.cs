using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RacedayAPI.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        public int Position { get; set; }

        public bool IsDisqualified { get; set; } = false;

        [MaxLength(255)]
        public string? Notes { get; set; }

        // Navigation property
        [ForeignKey("EnrolmentID")]
        public Enrolment? Enrolment { get; set; }
    }
}