using psi25_project.Models;
using psi25_project.Repositories.Interfaces;
using psi25_project.Data;
using Microsoft.EntityFrameworkCore;

namespace psi25_project.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly GeoHuntContext _context;

        public RoomRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<Room?> GetRoomByCodeAsync(string roomCode)
        {
            return await _context.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
        }

        public async Task<Room> CreateRoomAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}