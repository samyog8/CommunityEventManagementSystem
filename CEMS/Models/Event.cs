using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityEventSystem.Models.Interfaces;

namespace CommunityEventSystem.Models
{
    public class Event : BaseEntity, IRegistrable, IManageable
    {
        [Required(ErrorMessage = "Event name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Event name must be between 3 and 200 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        
        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }
        
        [Required(ErrorMessage = "End time is required")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
        
        [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10000")]
        public int MaxCapacity { get; set; } = 100;
        
        public bool IsActive { get; set; } = true;
        
        public int? VenueId { get; set; }
        
        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }
        
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        
        public virtual ICollection<EventActivity> EventActivities { get; set; } = new List<EventActivity>();
        
        public bool CanRegister()
        {
            return IsActive && Date >= DateTime.UtcNow.Date && GetAvailableSpots() > 0;
        }
        
        public int GetAvailableSpots()
        {
            return MaxCapacity - Registrations.Count(r => r.Status != RegistrationStatus.Cancelled);
        }
        
        public void Activate() => IsActive = true;
        
        public void Deactivate() => IsActive = false;
        
        [NotMapped]
        public DateTime StartDateTime => Date.Add(StartTime);
        
        [NotMapped]
        public DateTime EndDateTime => Date.Add(EndTime);
    }
}
