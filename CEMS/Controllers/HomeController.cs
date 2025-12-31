using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommunityEventSystem.Data;
using CommunityEventSystem.Services.Interfaces;

namespace CommunityEventSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ApplicationDbContext _context;
        
        public HomeController(IEventService eventService, ApplicationDbContext context)
        {
            _eventService = eventService;
            _context = context;
        }
        
        public async Task<IActionResult> Index()
        {
            var upcomingEvents = (await _eventService.GetUpcomingEventsAsync()).Take(6).ToList();
            ViewBag.UpcomingEvents = upcomingEvents;
            ViewBag.TotalEvents = await _context.Events.CountAsync(e => e.IsActive && e.Date >= DateTime.UtcNow.Date);
            ViewBag.TotalVenues = await _context.Venues.CountAsync(v => v.IsActive);
            ViewBag.TotalParticipants = await _context.Participants.CountAsync();
            return View();
        }
        
        public IActionResult Error()
        {
            return View();
        }
    }
}
