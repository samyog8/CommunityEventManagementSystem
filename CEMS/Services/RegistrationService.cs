using Microsoft.EntityFrameworkCore;
using CommunityEventSystem.Data;
using CommunityEventSystem.Models;
using CommunityEventSystem.Models.ViewModels;
using CommunityEventSystem.Services.Interfaces;
using CommunityEventSystem.Exceptions;

namespace CommunityEventSystem.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;
        
        public RegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<Registration?> GetRegistrationByIdAsync(int id)
        {
            return await _context.Registrations
                .Include(r => r.Participant)
                .Include(r => r.Event)
                    .ThenInclude(e => e.Venue)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        
        public async Task<IEnumerable<Registration>> GetRegistrationsByEventAsync(int eventId)
        {
            return await _context.Registrations
                .Include(r => r.Participant)
                .Where(r => r.EventId == eventId)
                .OrderBy(r => r.RegistrationDate)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Registration>> GetRegistrationsByParticipantAsync(int participantId)
        {
            return await _context.Registrations
                .Include(r => r.Event)
                    .ThenInclude(e => e.Venue)
                .Where(r => r.ParticipantId == participantId)
                .OrderByDescending(r => r.Event.Date)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Registration>> GetRegistrationsByEmailAsync(string email)
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower());
            
            if (participant == null)
            {
                return Enumerable.Empty<Registration>();
            }
            
            return await GetRegistrationsByParticipantAsync(participant.Id);
        }
        
        public async Task<Registration> RegisterForEventAsync(RegistrationCreateViewModel model)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == model.EventId);
            
            if (eventEntity == null)
            {
                throw new NotFoundException($"Event with ID {model.EventId} not found");
            }
            
            if (!eventEntity.CanRegister())
            {
                throw new ValidationException("This event is not available for registration");
            }
            
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.Email.ToLower() == model.Email.ToLower());
            
            if (participant == null)
            {
                participant = new Participant
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };
                _context.Participants.Add(participant);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existingRegistration = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.ParticipantId == participant.Id && 
                                              r.EventId == model.EventId &&
                                              r.Status != RegistrationStatus.Cancelled);
                
                if (existingRegistration != null)
                {
                    throw new ValidationException("You are already registered for this event");
                }
            }
            
            var registration = new Registration
            {
                ParticipantId = participant.Id,
                EventId = model.EventId,
                Notes = model.Notes,
                Status = RegistrationStatus.Pending
            };
            
            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();
            
            return registration;
        }
        
        public async Task<bool> CancelRegistrationAsync(int registrationId)
        {
            var registration = await _context.Registrations.FindAsync(registrationId);
            if (registration == null) return false;
            
            registration.Cancel();
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> ConfirmRegistrationAsync(int registrationId)
        {
            var registration = await _context.Registrations.FindAsync(registrationId);
            if (registration == null) return false;
            
            registration.Confirm();
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> MarkAttendedAsync(int registrationId)
        {
            var registration = await _context.Registrations.FindAsync(registrationId);
            if (registration == null) return false;
            
            registration.MarkAttended();
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> RejectRegistrationAsync(int registrationId)
        {
            var registration = await _context.Registrations.FindAsync(registrationId);
            if (registration == null) return false;
            
            registration.Reject();
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<Registration>> GetPendingRegistrationsAsync()
        {
            return await _context.Registrations
                .Include(r => r.Participant)
                .Include(r => r.Event)
                    .ThenInclude(e => e.Venue)
                .Where(r => r.Status == RegistrationStatus.Pending)
                .OrderBy(r => r.RegistrationDate)
                .ToListAsync();
        }
        
        public async Task<bool> IsAlreadyRegisteredAsync(string email, int eventId)
        {
            return await _context.Registrations
                .Include(r => r.Participant)
                .AnyAsync(r => r.Participant.Email.ToLower() == email.ToLower() && 
                              r.EventId == eventId &&
                              r.Status != RegistrationStatus.Cancelled);
        }
    }
}
