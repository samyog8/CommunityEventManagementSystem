using Microsoft.EntityFrameworkCore;
using CommunityEventSystem.Data;
using CommunityEventSystem.Models;
using CommunityEventSystem.Models.ViewModels;
using CommunityEventSystem.Services.Interfaces;
using CommunityEventSystem.Exceptions;

namespace CommunityEventSystem.Services
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;
        
        public EventService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Registrations)
                .Include(e => e.EventActivities)
                    .ThenInclude(ea => ea.Activity)
                .OrderByDescending(e => e.Date)
                .ThenBy(e => e.StartTime)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Registrations)
                .Include(e => e.EventActivities)
                    .ThenInclude(ea => ea.Activity)
                .Where(e => e.IsActive && e.Date >= DateTime.UtcNow.Date)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.StartTime)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Event>> FilterEventsAsync(EventFilterViewModel filter)
        {
            var query = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Registrations)
                .Include(e => e.EventActivities)
                    .ThenInclude(ea => ea.Activity)
                .AsQueryable();
            
            if (!filter.ShowPastEvents)
            {
                query = query.Where(e => e.Date >= DateTime.UtcNow.Date);
            }
            
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(e => 
                    e.Name.ToLower().Contains(term) || 
                    e.Description.ToLower().Contains(term));
            }
            
            if (filter.DateFrom.HasValue)
            {
                query = query.Where(e => e.Date >= filter.DateFrom.Value);
            }
            
            if (filter.DateTo.HasValue)
            {
                query = query.Where(e => e.Date <= filter.DateTo.Value);
            }
            
            if (filter.VenueId.HasValue)
            {
                query = query.Where(e => e.VenueId == filter.VenueId.Value);
            }
            
            if (filter.ActivityType.HasValue)
            {
                query = query.Where(e => e.EventActivities.Any(ea => 
                    ea.Activity.Type == filter.ActivityType.Value));
            }
            
            return await query
                .Where(e => e.IsActive)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.StartTime)
                .ToListAsync();
        }
        
        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Registrations)
                    .ThenInclude(r => r.Participant)
                .Include(e => e.EventActivities)
                    .ThenInclude(ea => ea.Activity)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
        
        public async Task<Event> CreateEventAsync(EventCreateViewModel model)
        {
            if (model.EndTime <= model.StartTime)
            {
                throw new ValidationException("End time must be after start time");
            }
            
            if (model.Date < DateTime.UtcNow.Date)
            {
                throw new ValidationException("Event date cannot be in the past");
            }
            
            var eventEntity = new Event
            {
                Name = model.Name,
                Description = model.Description,
                Date = model.Date,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                MaxCapacity = model.MaxCapacity,
                VenueId = model.VenueId,
                IsActive = true
            };
            
            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();
            
            if (model.SelectedActivityIds.Any())
            {
                foreach (var activityId in model.SelectedActivityIds)
                {
                    _context.EventActivities.Add(new EventActivity
                    {
                        EventId = eventEntity.Id,
                        ActivityId = activityId
                    });
                }
                await _context.SaveChangesAsync();
            }
            
            return eventEntity;
        }
        
        public async Task<Event> UpdateEventAsync(EventEditViewModel model)
        {
            var eventEntity = await _context.Events
                .Include(e => e.EventActivities)
                .FirstOrDefaultAsync(e => e.Id == model.Id);
            
            if (eventEntity == null)
            {
                throw new NotFoundException($"Event with ID {model.Id} not found");
            }
            
            if (model.EndTime <= model.StartTime)
            {
                throw new ValidationException("End time must be after start time");
            }
            
            eventEntity.Name = model.Name;
            eventEntity.Description = model.Description;
            eventEntity.Date = model.Date;
            eventEntity.StartTime = model.StartTime;
            eventEntity.EndTime = model.EndTime;
            eventEntity.MaxCapacity = model.MaxCapacity;
            eventEntity.VenueId = model.VenueId;
            eventEntity.IsActive = model.IsActive;
            
            _context.EventActivities.RemoveRange(eventEntity.EventActivities);
            
            foreach (var activityId in model.SelectedActivityIds)
            {
                _context.EventActivities.Add(new EventActivity
                {
                    EventId = eventEntity.Id,
                    ActivityId = activityId
                });
            }
            
            await _context.SaveChangesAsync();
            return eventEntity;
        }
        
        public async Task<bool> DeleteEventAsync(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null) return false;
            
            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> ToggleEventStatusAsync(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null) return false;
            
            if (eventEntity.IsActive)
                eventEntity.Deactivate();
            else
                eventEntity.Activate();
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> AddActivityToEventAsync(int eventId, int activityId)
        {
            var exists = await _context.EventActivities
                .AnyAsync(ea => ea.EventId == eventId && ea.ActivityId == activityId);
            
            if (exists) return false;
            
            _context.EventActivities.Add(new EventActivity
            {
                EventId = eventId,
                ActivityId = activityId
            });
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> RemoveActivityFromEventAsync(int eventId, int activityId)
        {
            var eventActivity = await _context.EventActivities
                .FirstOrDefaultAsync(ea => ea.EventId == eventId && ea.ActivityId == activityId);
            
            if (eventActivity == null) return false;
            
            _context.EventActivities.Remove(eventActivity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
