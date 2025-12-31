using System.ComponentModel.DataAnnotations;

namespace CommunityEventSystem.Models.ViewModels
{
    public class EventCreateViewModel
    {
        [Required(ErrorMessage = "Event name is required")]
        [StringLength(200, MinimumLength = 3)]
        [Display(Name = "Event Name")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date.AddDays(7);
        
        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);
        
        [Required(ErrorMessage = "End time is required")]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);
        
        [Range(1, 10000)]
        [Display(Name = "Maximum Capacity")]
        public int MaxCapacity { get; set; } = 100;
        
        [Display(Name = "Venue")]
        public int? VenueId { get; set; }
        
        [Display(Name = "Activities")]
        public List<int> SelectedActivityIds { get; set; } = new();
        
        public List<Venue> AvailableVenues { get; set; } = new();
        public List<Activity> AvailableActivities { get; set; } = new();
    }
    
    public class EventEditViewModel : EventCreateViewModel
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
    }
    
    public class EventFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? VenueId { get; set; }
        public ActivityType? ActivityType { get; set; }
        public bool ShowPastEvents { get; set; } = false;
        
        public List<Venue> AvailableVenues { get; set; } = new();
        public List<Event> Events { get; set; } = new();
    }
    
    public class EventDetailsViewModel
    {
        public Event Event { get; set; } = null!;
        public bool CanRegister { get; set; }
        public bool IsRegistered { get; set; }
        public Registration? CurrentRegistration { get; set; }
        public int AvailableSpots { get; set; }
    }
}
