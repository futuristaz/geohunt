using psi25_project.Models;
using System.Threading.Tasks;

namespace psi25_project.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room?> GetRoomByIdAsync(Guid roomId);
        Task<Room?> GetRoomByCodeAsync(string roomCode);
        Task<Room> CreateRoomAsync(Room room);
        Task<Room?> DeleteRoomAsync(Guid roomId);
        Task<Room?> GetRoomWithPlayersAsync(string roomCode);

    }
}
