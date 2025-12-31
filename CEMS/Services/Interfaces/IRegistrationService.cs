using CommunityEventSystem.Models;
using CommunityEventSystem.Models.ViewModels;

namespace CommunityEventSystem.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<Registration?> GetRegistrationByIdAsync(int id);
        Task<IEnumerable<Registration>> GetRegistrationsByEventAsync(int eventId);
        Task<IEnumerable<Registration>> GetRegistrationsByParticipantAsync(int participantId);
        Task<IEnumerable<Registration>> GetRegistrationsByEmailAsync(string email);
        Task<IEnumerable<Registration>> GetPendingRegistrationsAsync();
        Task<Registration> RegisterForEventAsync(RegistrationCreateViewModel model);
        Task<bool> CancelRegistrationAsync(int registrationId);
        Task<bool> ConfirmRegistrationAsync(int registrationId);
        Task<bool> RejectRegistrationAsync(int registrationId);
        Task<bool> MarkAttendedAsync(int registrationId);
        Task<bool> IsAlreadyRegisteredAsync(string email, int eventId);
    }
}
