using System.ComponentModel.DataAnnotations;
using CommunityEventSystem.Models.Interfaces;

namespace CommunityEventSystem.Models
{
    public class Participant : BaseEntity, IManageable
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        public bool IsAdmin { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        
        public string FullName => $"{FirstName} {LastName}";
        
        public void Activate() => IsActive = true;
        
        public void Deactivate() => IsActive = false;
        
        public int GetRegistrationCount()
        {
            return Registrations.Count(r => r.Status != RegistrationStatus.Cancelled);
        }
    }
}
