using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityEventSystem.Models
{
    public enum RegistrationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Attended,
        Rejected
    }
    
    public class Registration : BaseEntity
    {
        [Required]
        public int ParticipantId { get; set; }
        
        [ForeignKey("ParticipantId")]
        public virtual Participant Participant { get; set; } = null!;
        
        [Required]
        public int EventId { get; set; }
        
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;
        
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public void Confirm()
        {
            if (Status == RegistrationStatus.Pending)
            {
                Status = RegistrationStatus.Confirmed;
                UpdateTimestamp();
            }
        }
        
        public void Cancel()
        {
            if (Status != RegistrationStatus.Attended)
            {
                Status = RegistrationStatus.Cancelled;
                UpdateTimestamp();
            }
        }
        
        public void MarkAttended()
        {
            if (Status == RegistrationStatus.Confirmed)
            {
                Status = RegistrationStatus.Attended;
                UpdateTimestamp();
            }
        }
        
        public void Reject()
        {
            if (Status == RegistrationStatus.Pending)
            {
                Status = RegistrationStatus.Rejected;
                UpdateTimestamp();
            }
        }
        
        public bool CanBeApproved => Status == RegistrationStatus.Pending;
        public bool CanBeRejected => Status == RegistrationStatus.Pending;
    }
}
