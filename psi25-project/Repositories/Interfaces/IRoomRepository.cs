using psi25_project.Models;

namespace psi25_project.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room?> GetRoomByCodeAsync(string roomCode);
        Task<Room> CreateRoomAsync(Room room);
        Task SaveChangesAsync();
    }    
}