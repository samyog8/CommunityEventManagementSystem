using CommunityEventSystem.Models.Interfaces;

namespace CommunityEventSystem.Services
{
    public static class EntityManager
    {
        public static void ToggleStatus(IManageable entity)
        {
            if (entity.IsActive)
                entity.Deactivate();
            else
                entity.Activate();
        }
        
        public static void ActivateAll(IEnumerable<IManageable> entities)
        {
            foreach (var entity in entities)
            {
                entity.Activate();
            }
        }
        
        public static void DeactivateAll(IEnumerable<IManageable> entities)
        {
            foreach (var entity in entities)
            {
                entity.Deactivate();
            }
        }
        
        public static int CountActive(IEnumerable<IManageable> entities)
        {
            return entities.Count(e => e.IsActive);
        }
        
        public static string GetAvailabilityStatus(IRegistrable registrable)
        {
            if (!registrable.CanRegister())
                return "Unavailable";
                
            var spots = registrable.GetAvailableSpots();
            if (spots <= 5)
                return $"Only {spots} spots left!";
            else if (spots <= 20)
                return $"{spots} spots available";
            else
                return "Available";
        }
    }
}
