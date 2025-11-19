using psi25_project.Repositories;
using psi25_project.Repositories.Interfaces;
using psi25_project.Models;

namespace psi25_project.Services
{
    public class RoomService
    {
        private readonly IRoomRepository _rooms;
        private readonly IPlayerRepository _players;

        public RoomService(IRoomRepository rooms, IPlayerRepository players)
        {
            _rooms = rooms;
            _players = players;
        }

        public async Task<Room> CreateRoomAsync()
        {
            var room = new Room
            {
                RoomCode = GenerateCode(),
                CreatedAt = DateTime.UtcNow
            };

            return await _rooms.CreateRoomAsync(room);
        }

        public async Task<Player?> JoinRoomAsync(string roomCode, Guid userId, string displayName)
        {
            var room = await _rooms.GetRoomByCodeAsync(roomCode);
            if (room == null) return null;

            var player = new Player
            {
                UserId = userId,
                RoomId = room.Id,
                DisplayName = displayName,
                Score = 0,
                IsReady = false
            };

            await _players.AddPlayerAsync(player);
            return player;
        }

        private string GenerateCode()
        {
            return Guid.NewGuid().ToString("N")[..5].ToUpper();
        }
    }

}