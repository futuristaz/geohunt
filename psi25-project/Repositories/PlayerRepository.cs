using psi25_project.Models;
using psi25_project.Repositories.Interfaces;
using psi25_project.Data;
using Microsoft.EntityFrameworkCore;

namespace psi25_project.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly GeoHuntContext _context;

        public PlayerRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<Player> AddPlayerAsync(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return player;
        }

        public async Task<List<Player>> GetPlayersInRoomAsync(Guid roomId)
        {
            return await _context.Players
                .Where(p => p.RoomId == roomId)
                .Include(p => p.User)
                .ToListAsync();
        }
    }
}