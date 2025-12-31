using System.ComponentModel.DataAnnotations;

namespace CommunityEventSystem.Models.ViewModels
{
    public class RegistrationCreateViewModel
    {
        public int EventId { get; set; }
        public Event? Event { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
    
    public class MyRegistrationsViewModel
    {
        public string Email { get; set; } = string.Empty;
        public Participant? Participant { get; set; }
        public List<Registration> Registrations { get; set; } = new();
    }
}
