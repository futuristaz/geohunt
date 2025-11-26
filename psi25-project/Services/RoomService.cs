using psi25_project.Repositories;
using psi25_project.Repositories.Interfaces;
using psi25_project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // Create a new room (empty players)
       public async Task<Room> CreateRoomAsync(int totalRounds)
       {
            var room = new Room
            {
                RoomCode = GenerateCode(),
                CreatedAt = DateTime.UtcNow,
                TotalRounds = totalRounds,
                CurrentRounds = 1
            };

            return await _rooms.CreateRoomAsync(room);
        }


        // Join a room → user becomes player
        public async Task<Player?> JoinRoomAsync(string roomCode, Guid userId, string displayName)
        {
            var room = await _rooms.GetRoomByCodeAsync(roomCode);
            if (room == null) return null;

            // Prevent duplicate players in the same room
            var existingPlayer = await _players.GetPlayerByUserAndRoomAsync(userId, room.Id);
            if (existingPlayer != null)
                return existingPlayer;

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

        // Get all players in a specific room
        public async Task<List<Player>> GetPlayersInRoomAsync(string roomCode)
        {
            var room = await _rooms.GetRoomWithPlayersAsync(roomCode);
            return room?.Players.ToList() ?? new List<Player>();
        }

        private string GenerateCode()
        {
            return Guid.NewGuid().ToString("N")[..5].ToUpper();
        }

        public async Task<Player?> SetReadyAsync(Guid playerId)
        {
            var player = await _players.GetPlayerByIdAsync(playerId);
            if (player == null) return null;
            player.IsReady = true;
            await _players.UpdatePlayerAsync(player);
            return player;
        }

        public async Task<Player?> ToggleReadyAsync(Guid playerId)
        {
            var player = await _players.GetPlayerByIdAsync(playerId);
            if (player == null) return null;

            player.IsReady = !player.IsReady; // toggle manually
            await _players.UpdatePlayerAsync(player);
            return player;
        }

        public async Task<bool> LeaveRoomAsync(Guid playerId)
        {
            var player = await _players.GetPlayerByIdAsync(playerId);
            if (player == null) return false;

            player.IsReady = false;
            player.RoomId = null; // set to null instead of Guid.Empty

            await _players.UpdatePlayerAsync(player); // ensure this calls _context.SaveChangesAsync()
            return true;
        }


    }
}
