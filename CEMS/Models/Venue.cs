using System.ComponentModel.DataAnnotations;
using CommunityEventSystem.Models.Interfaces;

namespace CommunityEventSystem.Models
{
    public class Venue : BaseEntity, IManageable
    {
        [Required(ErrorMessage = "Venue name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Venue name must be between 3 and 200 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(20)]
        public string? PostCode { get; set; }
        
        [Range(1, 100000, ErrorMessage = "Capacity must be between 1 and 100000")]
        public int Capacity { get; set; } = 100;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
        
        public void Activate() => IsActive = true;
        
        public void Deactivate() => IsActive = false;
        
        public bool IsAvailable(DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeEventId = null)
        {
            return !Events.Any(e => 
                e.Id != excludeEventId &&
                e.Date == date &&
                e.IsActive &&
                ((startTime >= e.StartTime && startTime < e.EndTime) ||
                 (endTime > e.StartTime && endTime <= e.EndTime) ||
                 (startTime <= e.StartTime && endTime >= e.EndTime)));
        }
        
        public string FullAddress => string.IsNullOrEmpty(City) 
            ? Address 
            : $"{Address}, {City}{(string.IsNullOrEmpty(PostCode) ? "" : $" {PostCode}")}";
    }
}
