using System.ComponentModel.DataAnnotations;
using CommunityEventSystem.Models.Interfaces;

namespace CommunityEventSystem.Models
{
    public enum ActivityType
    {
        Workshop,
        Talk,
        Game,
        Performance,
        Exhibition,
        Networking,
        Other
    }
    
    public class Activity : BaseEntity, IManageable
    {
        [Required(ErrorMessage = "Activity name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Activity name must be between 3 and 200 characters")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public ActivityType Type { get; set; } = ActivityType.Other;
        
        [Range(0, 1440, ErrorMessage = "Duration must be between 0 and 1440 minutes")]
        public int DurationMinutes { get; set; } = 60;
        
        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<EventActivity> EventActivities { get; set; } = new List<EventActivity>();
        
        public void Activate() => IsActive = true;
        
        public void Deactivate() => IsActive = false;
        
        public string DurationDisplay
        {
            get
            {
                if (DurationMinutes < 60) return $"{DurationMinutes} min";
                var hours = DurationMinutes / 60;
                var mins = DurationMinutes % 60;
                return mins > 0 ? $"{hours}h {mins}m" : $"{hours}h";
            }
        }
    }
}
