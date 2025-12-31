using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using CommunityEventSystem.Data;
using CommunityEventSystem.Models;

namespace CommunityEventSystem.Filters
{
    public class AdminLayoutFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _context;

        public AdminLayoutFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is Controller controller)
            {
                var pendingCount = await _context.Registrations
                    .CountAsync(r => r.Status == RegistrationStatus.Pending);
                controller.ViewBag.PendingCount = pendingCount;
            }

            await next();
        }
    }
}
