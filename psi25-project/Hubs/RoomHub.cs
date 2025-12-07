using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psi25_project.Hubs
{
    public class RoomHub : Hub
    {
        private readonly IRoomOnlineService _roomOnlineService;
        private readonly GeoHuntContext _context; // ✅ Add this

        public RoomHub(IRoomOnlineService roomOnlineService, GeoHuntContext context)
        {
            _roomOnlineService = roomOnlineService;
            _context = context; // ✅ Add this
        }

        // Player joins a room
        public async Task JoinRoom(string roomCode, Guid playerId, string displayName)
        {
            var connectionId = Context.ConnectionId;
            
            // Get the actual room from database
            var room = await _context.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
                
            if (room == null)
            {
                throw new HubException("Room not found");
            }
            
            var roomId = room.Id.ToString();
            await Groups.AddToGroupAsync(connectionId, roomId);
            
            // Remove any existing connection for this player
            var existingPlayer = _roomOnlineService.GetOnlinePlayers(roomId)
                .FirstOrDefault(p => p.PlayerId == playerId);
            
            if (existingPlayer != null)
            {
                Console.WriteLine($"Player {playerId} already in room, removing old connection");
                _roomOnlineService.RemoveOnlinePlayer(roomId, existingPlayer.ConnectionId);
            }
            
            // Add player with new connection
            _roomOnlineService.AddOnlinePlayer(roomId, new PlayerOnline
            {
                PlayerId = playerId,
                DisplayName = displayName,
                ConnectionId = connectionId,
                IsReady = false
            });

            Console.WriteLine($"Player {displayName} joined room {roomCode}");

            // ✅ Broadcast the actual database players (with correct ready states)
            var dbPlayers = room.Players.Select(p => new
            {
                id = p.Id.ToString(),
                userId = p.UserId.ToString(),
                displayName = p.DisplayName,
                isReady = p.IsReady
            }).ToList();

            await Clients.Group(roomId).SendAsync("PlayerListUpdated", dbPlayers);
        }

        // Player leaves manually
        public async Task LeaveRoom(string roomCode)
        {
            var connectionId = Context.ConnectionId;
            
            // Get room from database
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
                
            if (room == null) return;
            
            var roomId = room.Id.ToString();
            
            var removedPlayer = _roomOnlineService.RemoveOnlinePlayer(roomId, connectionId);
            if (removedPlayer != null)
            {
                await Clients.Group(roomId).SendAsync("PlayerLeft", removedPlayer.PlayerId);
                
                // Broadcast updated list from database
                var dbRoom = await _context.Rooms
                    .Include(r => r.Players)
                    .FirstOrDefaultAsync(r => r.Id == room.Id);
                    
                if (dbRoom != null)
                {
                    var dbPlayers = dbRoom.Players.Select(p => new
                    {
                        id = p.Id.ToString(),
                        userId = p.UserId.ToString(),
                        displayName = p.DisplayName,
                        isReady = p.IsReady
                    }).ToList();

                    await Clients.Group(roomId).SendAsync("PlayerListUpdated", dbPlayers);
                }
            }

            await Groups.RemoveFromGroupAsync(connectionId, roomId);
        }

        // Toggle ready/unready
        public async Task UpdateReadyState(string roomCode, Guid playerId, bool isReady)
        {
            // Get room from database
            var room = await _context.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
                
            if (room == null) return;
            
            var roomId = room.Id.ToString();
            
            // Update in-memory online service
            _roomOnlineService.UpdatePlayerState(roomId, playerId, isReady);
            
            // Broadcast database players (source of truth)
            var dbPlayers = room.Players.Select(p => new
            {
                id = p.Id.ToString(),
                userId = p.UserId.ToString(),
                displayName = p.DisplayName,
                isReady = p.IsReady
            }).ToList();

            await Clients.Group(roomId).SendAsync("PlayerListUpdated", dbPlayers);
        }

        // Handle disconnect
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var roomIds = _roomOnlineService.GetRoomsForConnection(connectionId);
            
            foreach (var roomId in roomIds)
            {
                var removedPlayer = _roomOnlineService.RemoveOnlinePlayer(roomId, connectionId);
                if (removedPlayer != null)
                {
                    await Clients.Group(roomId).SendAsync("PlayerLeft", removedPlayer.PlayerId);
                    
                    // Fetch and broadcast database players
                    var room = await _context.Rooms
                        .Include(r => r.Players)
                        .FirstOrDefaultAsync(r => r.Id.ToString() == roomId);
                        
                    if (room != null)
                    {
                        var dbPlayers = room.Players.Select(p => new
                        {
                            id = p.Id.ToString(),
                            userId = p.UserId.ToString(),
                            displayName = p.DisplayName,
                            isReady = p.IsReady
                        }).ToList();

                        await Clients.Group(roomId).SendAsync("PlayerListUpdated", dbPlayers);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}