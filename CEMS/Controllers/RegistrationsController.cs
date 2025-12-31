using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommunityEventSystem.Data;
using CommunityEventSystem.Models;
using CommunityEventSystem.Models.ViewModels;
using CommunityEventSystem.Services.Interfaces;
using CommunityEventSystem.Exceptions;

namespace CommunityEventSystem.Controllers
{
    public class RegistrationsController : Controller
    {
        private readonly IRegistrationService _registrationService;
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;
        
        public RegistrationsController(
            IRegistrationService registrationService,
            IEventService eventService,
            ApplicationDbContext context)
        {
            _registrationService = registrationService;
            _eventService = eventService;
            _context = context;
        }
        
        public async Task<IActionResult> Register(int eventId)
        {
            var eventEntity = await _eventService.GetEventByIdAsync(eventId);
            if (eventEntity == null)
            {
                return NotFound();
            }
            
            if (!eventEntity.CanRegister())
            {
                TempData["Error"] = "This event is not available for registration.";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }
            
            var viewModel = new RegistrationCreateViewModel
            {
                EventId = eventId,
                Event = eventEntity
            };
            
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Event = await _eventService.GetEventByIdAsync(model.EventId);
                return View(model);
            }
            
            try
            {
                var registration = await _registrationService.RegisterForEventAsync(model);
                TempData["Success"] = "You have been successfully registered for the event!";
                return RedirectToAction("Confirmation", new { id = registration.Id });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Event = await _eventService.GetEventByIdAsync(model.EventId);
                return View(model);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
        
        public async Task<IActionResult> Confirmation(int id)
        {
            var registration = await _registrationService.GetRegistrationByIdAsync(id);
            if (registration == null)
            {
                return NotFound();
            }
            return View(registration);
        }
        
        public IActionResult MyRegistrations()
        {
            return View(new MyRegistrationsViewModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyRegistrations(MyRegistrationsViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required");
                return View(model);
            }
            
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.Email.ToLower() == model.Email.ToLower());
            
            model.Participant = participant;
            model.Registrations = (await _registrationService.GetRegistrationsByEmailAsync(model.Email)).ToList();
            
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string returnEmail)
        {
            var result = await _registrationService.CancelRegistrationAsync(id);
            if (result)
            {
                TempData["Success"] = "Registration cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to cancel registration.";
            }
            
            if (!string.IsNullOrEmpty(returnEmail))
            {
                return RedirectToAction("MyRegistrations", new { email = returnEmail });
            }
            return RedirectToAction("Index", "Events");
        }
    }
}
