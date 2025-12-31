using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommunityEventSystem.Data;
using CommunityEventSystem.Models;
using CommunityEventSystem.Models.ViewModels;
using CommunityEventSystem.Services;
using CommunityEventSystem.Services.Interfaces;
using CommunityEventSystem.Exceptions;
using CommunityEventSystem.Filters;

namespace CommunityEventSystem.Controllers
{
    [Route("Admin")]
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(AdminLayoutFilter))]
    public class AdminController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IRegistrationService _registrationService;
        private readonly ApplicationDbContext _context;
        
        public AdminController(
            IEventService eventService,
            IRegistrationService registrationService,
            ApplicationDbContext context)
        {
            _eventService = eventService;
            _registrationService = registrationService;
            _context = context;
        }
        
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewBag.EventCount = await _context.Events.CountAsync();
            ViewBag.ParticipantCount = await _context.Participants.CountAsync();
            ViewBag.VenueCount = await _context.Venues.CountAsync();
            ViewBag.ActivityCount = await _context.Activities.CountAsync();
            ViewBag.RegistrationCount = await _context.Registrations.CountAsync();
            ViewBag.PendingRegistrationCount = await _context.Registrations.CountAsync(r => r.Status == RegistrationStatus.Pending);
            return View();
        }
        
        [HttpGet("Events")]
        public async Task<IActionResult> Events()
        {
            var events = await _eventService.GetAllEventsAsync();
            return View(events);
        }
        
        [HttpGet("Events/Create")]
        public async Task<IActionResult> CreateEvent()
        {
            var model = new EventCreateViewModel
            {
                AvailableVenues = await _context.Venues.Where(v => v.IsActive).ToListAsync(),
                AvailableActivities = await _context.Activities.Where(a => a.IsActive).ToListAsync()
            };
            return View(model);
        }
        
        [HttpPost("Events/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableVenues = await _context.Venues.Where(v => v.IsActive).ToListAsync();
                model.AvailableActivities = await _context.Activities.Where(a => a.IsActive).ToListAsync();
                return View(model);
            }
            
            try
            {
                await _eventService.CreateEventAsync(model);
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction("Events");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.AvailableVenues = await _context.Venues.Where(v => v.IsActive).ToListAsync();
                model.AvailableActivities = await _context.Activities.Where(a => a.IsActive).ToListAsync();
                return View(model);
            }
        }
        
        [HttpGet("Events/Edit/{id}")]
        public async Task<IActionResult> EditEvent(int id)
        {
            var eventEntity = await _eventService.GetEventByIdAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }
            
            var model = new EventEditViewModel
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                Date = eventEntity.Date,
                StartTime = eventEntity.StartTime,
                EndTime = eventEntity.EndTime,
                MaxCapacity = eventEntity.MaxCapacity,
                VenueId = eventEntity.VenueId,
                IsActive = eventEntity.IsActive,
                SelectedActivityIds = eventEntity.EventActivities.Select(ea => ea.ActivityId).ToList(),
                AvailableVenues = await _context.Venues.Where(v => v.IsActive).ToListAsync(),
                AvailableActivities = await _context.Activities.Where(a => a.IsActive).ToListAsync()
            };
            
            return View(model);
        }
        
        [HttpPost("Events/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, EventEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                model.AvailableVenues = await _context.Venues.Where(v => v.IsActive).ToListAsync();
                model.AvailableActivities = await _context.Activities.Where(a => a.IsActive).ToListAsync();
                return View(model);
            }
            
            try
            {
                await _eventService.UpdateEventAsync(model);
                TempData["Success"] = "Event updated successfully!";
                return RedirectToAction("Events");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.AvailableVenues = await _context.Venues.Where(v => v.IsActive).ToListAsync();
                model.AvailableActivities = await _context.Activities.Where(a => a.IsActive).ToListAsync();
                return View(model);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
        
        [HttpPost("Events/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await _eventService.DeleteEventAsync(id);
            TempData["Success"] = "Event deleted successfully!";
            return RedirectToAction("Events");
        }
        
        [HttpPost("Events/Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEvent(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity != null)
            {
                EntityManager.ToggleStatus(eventEntity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Events");
        }
        
        [HttpGet("Events/{eventId}/Registrations")]
        public async Task<IActionResult> EventRegistrations(int eventId)
        {
            var eventEntity = await _eventService.GetEventByIdAsync(eventId);
            if (eventEntity == null)
            {
                return NotFound();
            }
            return View(eventEntity);
        }
        
        [HttpPost("Registrations/Confirm/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmRegistration(int id, int eventId)
        {
            await _registrationService.ConfirmRegistrationAsync(id);
            return RedirectToAction("EventRegistrations", new { eventId });
        }
        
        [HttpPost("Registrations/Cancel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRegistration(int id, int eventId)
        {
            await _registrationService.CancelRegistrationAsync(id);
            return RedirectToAction("EventRegistrations", new { eventId });
        }
        
        [HttpPost("Registrations/Attended/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttended(int id, int eventId)
        {
            await _registrationService.MarkAttendedAsync(id);
            return RedirectToAction("EventRegistrations", new { eventId });
        }
        
        [HttpPost("Registrations/Reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRegistration(int id, int eventId)
        {
            await _registrationService.RejectRegistrationAsync(id);
            TempData["Success"] = "Registration rejected.";
            return RedirectToAction("EventRegistrations", new { eventId });
        }
        
        [HttpGet("PendingRegistrations")]
        public async Task<IActionResult> PendingRegistrations()
        {
            var pendingRegistrations = await _registrationService.GetPendingRegistrationsAsync();
            return View(pendingRegistrations);
        }
        
        [HttpPost("Registrations/ApproveFromPending/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFromPending(int id)
        {
            await _registrationService.ConfirmRegistrationAsync(id);
            TempData["Success"] = "Registration approved successfully!";
            return RedirectToAction("PendingRegistrations");
        }
        
        [HttpPost("Registrations/RejectFromPending/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectFromPending(int id)
        {
            await _registrationService.RejectRegistrationAsync(id);
            TempData["Success"] = "Registration rejected.";
            return RedirectToAction("PendingRegistrations");
        }
        
        [HttpGet("Venues")]
        public async Task<IActionResult> Venues()
        {
            var venues = await _context.Venues.Include(v => v.Events).ToListAsync();
            return View(venues);
        }
        
        [HttpGet("Venues/Create")]
        public IActionResult CreateVenue()
        {
            return View(new Venue());
        }
        
        [HttpPost("Venues/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVenue(Venue venue)
        {
            if (!ModelState.IsValid)
            {
                return View(venue);
            }
            
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Venue created successfully!";
            return RedirectToAction("Venues");
        }
        
        [HttpGet("Venues/Edit/{id}")]
        public async Task<IActionResult> EditVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }
        
        [HttpPost("Venues/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVenue(int id, Venue venue)
        {
            if (id != venue.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return View(venue);
            }
            
            _context.Update(venue);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Venue updated successfully!";
            return RedirectToAction("Venues");
        }
        
        [HttpPost("Venues/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null)
            {
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Venue deleted successfully!";
            }
            return RedirectToAction("Venues");
        }
        
        [HttpPost("Venues/Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null)
            {
                EntityManager.ToggleStatus(venue);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Venues");
        }
        
        [HttpGet("Activities")]
        public async Task<IActionResult> Activities()
        {
            var activities = await _context.Activities.Include(a => a.EventActivities).ToListAsync();
            return View(activities);
        }
        
        [HttpGet("Activities/Create")]
        public IActionResult CreateActivity()
        {
            return View(new Activity());
        }
        
        [HttpPost("Activities/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivity(Activity activity)
        {
            if (!ModelState.IsValid)
            {
                return View(activity);
            }
            
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Activity created successfully!";
            return RedirectToAction("Activities");
        }
        
        [HttpGet("Activities/Edit/{id}")]
        public async Task<IActionResult> EditActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                return NotFound();
            }
            return View(activity);
        }
        
        [HttpPost("Activities/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditActivity(int id, Activity activity)
        {
            if (id != activity.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return View(activity);
            }
            
            _context.Update(activity);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Activity updated successfully!";
            return RedirectToAction("Activities");
        }
        
        [HttpPost("Activities/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity != null)
            {
                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Activity deleted successfully!";
            }
            return RedirectToAction("Activities");
        }
        
        [HttpPost("Activities/Toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity != null)
            {
                EntityManager.ToggleStatus(activity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Activities");
        }
        
        [HttpGet("Participants")]
        public async Task<IActionResult> Participants()
        {
            var participants = await _context.Participants.Include(p => p.Registrations).ToListAsync();
            return View(participants);
        }
        
        [HttpGet("Participants/Edit/{id}")]
        public async Task<IActionResult> EditParticipant(int id)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant == null)
            {
                return NotFound();
            }
            return View(participant);
        }
        
        [HttpPost("Participants/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditParticipant(int id, Participant participant)
        {
            if (id != participant.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return View(participant);
            }
            
            _context.Update(participant);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Participant updated successfully!";
            return RedirectToAction("Participants");
        }
        
        [HttpPost("Participants/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteParticipant(int id)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant != null)
            {
                _context.Participants.Remove(participant);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Participant deleted successfully!";
            }
            return RedirectToAction("Participants");
        }
    }
}
