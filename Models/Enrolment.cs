using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RacedayAPI.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        public int ParticipantID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Registered";

        [Required]
        public int RaceNumber { get; set; }

        // Navigation properties
        [ForeignKey("ParticipantID")]
        public Participant? Participant { get; set; }

        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }

        public Result? Result { get; set; }
    }
}