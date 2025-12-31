using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommunityEventSystem.Data;
using CommunityEventSystem.Models;
using CommunityEventSystem.Models.ViewModels;
using CommunityEventSystem.Services.Interfaces;
using CommunityEventSystem.Exceptions;

namespace CommunityEventSystem.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;
        
        public EventsController(IEventService eventService, ApplicationDbContext context)
        {
            _eventService = eventService;
            _context = context;
        }
        
        public async Task<IActionResult> Index(EventFilterViewModel filter)
        {
            filter.AvailableVenues = await _context.Venues.Where(v => v.IsActive).ToListAsync();
            filter.Events = (await _eventService.FilterEventsAsync(filter)).ToList();
            return View(filter);
        }
        
        public async Task<IActionResult> Details(int id)
        {
            var eventEntity = await _eventService.GetEventByIdAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }
            
            var viewModel = new EventDetailsViewModel
            {
                Event = eventEntity,
                CanRegister = eventEntity.CanRegister(),
                AvailableSpots = eventEntity.GetAvailableSpots()
            };
            
            return View(viewModel);
        }
    }
}
