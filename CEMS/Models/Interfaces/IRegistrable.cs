namespace CommunityEventSystem.Models.Interfaces
{
    public interface IRegistrable
    {
        bool CanRegister();
        int GetAvailableSpots();
    }
}
