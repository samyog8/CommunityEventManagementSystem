using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityEventSystem.Models
{
    public class EventActivity : BaseEntity
    {
        [Required]
        public int EventId { get; set; }
        
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;
        
        [Required]
        public int ActivityId { get; set; }
        
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;
        
        [DataType(DataType.Time)]
        public TimeSpan? ScheduledTime { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public int DisplayOrder { get; set; } = 0;
    }
}
