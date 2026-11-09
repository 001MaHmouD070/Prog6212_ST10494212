using System.ComponentModel.DataAnnotations;

namespace Raceday.API.DTOs
{
    public class EnrolmentDto
    {
        [Required]
        public int CategoryID { get; set; }
    }
}