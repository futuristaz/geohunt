using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace psi25_project.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly GeoHuntContext _context;

        public PlayerRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task AddPlayerAsync(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
        }

        public async Task<Player?> GetPlayerByUserAndRoomAsync(Guid userId, Guid roomId)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.UserId == userId && p.RoomId == roomId);
        }

        public async Task<Player?> GetPlayerByIdAsync(Guid playerId)
        {
            return await _context.Players.FindAsync(playerId);
        }

        public async Task UpdatePlayerAsync(Player player)
        {
            _context.Players.Update(player);
            await _context.SaveChangesAsync();
        }
    }
}
