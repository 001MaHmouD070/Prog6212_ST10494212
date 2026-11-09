namespace RaceDay.Web.Models.ViewModels
{
    public class EventViewModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDateAndTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }

        // To show who is organizing it
        public string OrganiserName { get; set; } = string.Empty;
    }

    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal EntryFee { get; set; }
    }

    public class EventDetailsViewModel
    {
        public EventViewModel Event { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<EnrolmentViewModel> Enrolments { get; set; } = new(); // ⬅️ Add this line
    }

    public class EnrolmentViewModel
    {
        public int EnrolmentID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int RaceNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public string FinishTime { get; set; } = string.Empty;
        public int? Position { get; set; }
    }
}