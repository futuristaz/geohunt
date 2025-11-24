using psi25_project.Models;
using System.Threading.Tasks;

namespace psi25_project.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room> CreateRoomAsync(Room room);
        Task<Room?> GetRoomByCodeAsync(string roomCode);
        Task<Room?> GetRoomWithPlayersAsync(string roomCode);
    }
}
