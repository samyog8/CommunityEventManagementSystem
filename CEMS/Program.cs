using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using CommunityEventSystem.Data;
using CommunityEventSystem.Services;
using CommunityEventSystem.Services.Interfaces;
using CommunityEventSystem.Filters;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

var host = Environment.GetEnvironmentVariable("PGHOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
var database = Environment.GetEnvironmentVariable("PGDATABASE") ?? "CommunityEvents";
var username = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres";
var password = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "admin";

var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<AdminLayoutFilter>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    if (!context.Venues.Any())
    {
        context.Venues.AddRange(
            new CommunityEventSystem.Models.Venue { Name = "Community Hall", Address = "123 Main Street", City = "London", PostCode = "E1 1AB", Capacity = 200, Description = "Large community hall with stage" },
            new CommunityEventSystem.Models.Venue { Name = "City Park Pavilion", Address = "45 Park Lane", City = "Manchester", PostCode = "M1 2CD", Capacity = 150, Description = "Outdoor pavilion with seating" },
            new CommunityEventSystem.Models.Venue { Name = "Library Conference Room", Address = "78 High Street", City = "Birmingham", PostCode = "B2 3EF", Capacity = 50, Description = "Modern conference room" }
        );
        context.SaveChanges();
    }
    
    if (!context.Activities.Any())
    {
        context.Activities.AddRange(
            new CommunityEventSystem.Models.Activity { Name = "Photography Workshop", Description = "Learn basic photography skills", Type = CommunityEventSystem.Models.ActivityType.Workshop, DurationMinutes = 120 },
            new CommunityEventSystem.Models.Activity { Name = "Community Talk", Description = "Guest speaker presentation", Type = CommunityEventSystem.Models.ActivityType.Talk, DurationMinutes = 60 },
            new CommunityEventSystem.Models.Activity { Name = "Team Building Games", Description = "Fun group activities", Type = CommunityEventSystem.Models.ActivityType.Game, DurationMinutes = 90 },
            new CommunityEventSystem.Models.Activity { Name = "Art Exhibition", Description = "Local artists showcase", Type = CommunityEventSystem.Models.ActivityType.Exhibition, DurationMinutes = 180 },
            new CommunityEventSystem.Models.Activity { Name = "Networking Session", Description = "Meet and connect with others", Type = CommunityEventSystem.Models.ActivityType.Networking, DurationMinutes = 45 }
        );
        context.SaveChanges();
    }
    
    if (!context.Events.Any())
    {
        var venues = context.Venues.ToList();
        var activities = context.Activities.ToList();
        
        var event1 = new CommunityEventSystem.Models.Event
        {
            Name = "Summer Community Festival",
            Description = "Annual summer celebration with activities for all ages",
            Date = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(14), DateTimeKind.Utc),
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            MaxCapacity = 200,
            VenueId = venues[0].Id
        };
        
        var event2 = new CommunityEventSystem.Models.Event
        {
            Name = "Photography Workshop Day",
            Description = "Learn photography from local experts",
            Date = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(7), DateTimeKind.Utc),
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            MaxCapacity = 30,
            VenueId = venues[2].Id
        };
        
        var event3 = new CommunityEventSystem.Models.Event
        {
            Name = "Outdoor Movie Night",
            Description = "Family-friendly movie under the stars",
            Date = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(21), DateTimeKind.Utc),
            StartTime = new TimeSpan(19, 0, 0),
            EndTime = new TimeSpan(22, 0, 0),
            MaxCapacity = 100,
            VenueId = venues[1].Id
        };
        
        context.Events.AddRange(event1, event2, event3);
        context.SaveChanges();
        
        context.EventActivities.AddRange(
            new CommunityEventSystem.Models.EventActivity { EventId = event1.Id, ActivityId = activities[2].Id },
            new CommunityEventSystem.Models.EventActivity { EventId = event1.Id, ActivityId = activities[4].Id },
            new CommunityEventSystem.Models.EventActivity { EventId = event2.Id, ActivityId = activities[0].Id },
            new CommunityEventSystem.Models.EventActivity { EventId = event3.Id, ActivityId = activities[3].Id }
        );
        context.SaveChanges();
    }
}

app.Run();
