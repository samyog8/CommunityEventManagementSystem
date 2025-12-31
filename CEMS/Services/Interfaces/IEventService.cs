using CommunityEventSystem.Models;
using CommunityEventSystem.Models.ViewModels;

namespace CommunityEventSystem.Services.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<IEnumerable<Event>> FilterEventsAsync(EventFilterViewModel filter);
        Task<Event?> GetEventByIdAsync(int id);
        Task<Event> CreateEventAsync(EventCreateViewModel model);
        Task<Event> UpdateEventAsync(EventEditViewModel model);
        Task<bool> DeleteEventAsync(int id);
        Task<bool> ToggleEventStatusAsync(int id);
        Task<bool> AddActivityToEventAsync(int eventId, int activityId);
        Task<bool> RemoveActivityFromEventAsync(int eventId, int activityId);
    }
}
