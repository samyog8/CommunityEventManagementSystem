namespace CommunityEventSystem.Models.Interfaces
{
    public interface IManageable
    {
        bool IsActive { get; set; }
        void Activate();
        void Deactivate();
    }
}
