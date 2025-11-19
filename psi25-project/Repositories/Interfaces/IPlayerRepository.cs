using psi25_project.Models;

namespace psi25_project.Repositories.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player> AddPlayerAsync(Player player);
        Task<List<Player>> GetPlayersInRoomAsync(Guid roomId);
    }
}