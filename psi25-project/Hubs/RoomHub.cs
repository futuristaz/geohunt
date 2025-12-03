using Microsoft.AspNetCore.SignalR;
using psi25_project.Models;
using psi25_project.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace psi25_project.Hubs
{
    public class RoomHub : Hub
    {
        private readonly IRoomOnlineService _roomOnlineService;

        public RoomHub(IRoomOnlineService roomOnlineService)
        {
            _roomOnlineService = roomOnlineService;
        }

        // Player joins a room
        public async Task JoinRoom(string roomId, Guid playerId, string displayName)
        {
            var connectionId = Context.ConnectionId;

            await Groups.AddToGroupAsync(connectionId, roomId);

            _roomOnlineService.AddOnlinePlayer(roomId, new PlayerOnline
            {
                PlayerId = playerId,
                DisplayName = displayName,
                ConnectionId = connectionId,
                IsReady = false
            });

            await Clients.Group(roomId).SendAsync("PlayerJoined", displayName);

            var onlinePlayers = _roomOnlineService.GetOnlinePlayers(roomId);
            await Clients.Group(roomId).SendAsync("PlayerListUpdated", onlinePlayers);
        }

        // Player leaves manually
        public async Task LeaveRoom(string roomId)
        {
            var connectionId = Context.ConnectionId;
            var removedPlayer = _roomOnlineService.RemoveOnlinePlayer(roomId, connectionId);
            if (removedPlayer != null)
            {
                await Clients.Group(roomId).SendAsync("PlayerLeft", removedPlayer.PlayerId);

                var onlinePlayers = _roomOnlineService.GetOnlinePlayers(roomId);
                await Clients.Group(roomId).SendAsync("PlayerListUpdated", onlinePlayers);
            }

            await Groups.RemoveFromGroupAsync(connectionId, roomId);
        }

        // Toggle ready/unready
        public async Task UpdateReadyState(string roomId, Guid playerId, bool isReady)
        {
            _roomOnlineService.UpdatePlayerState(roomId, playerId, isReady);

            var onlinePlayers = _roomOnlineService.GetOnlinePlayers(roomId);
            await Clients.Group(roomId).SendAsync("PlayerListUpdated", onlinePlayers);
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

                    var onlinePlayers = _roomOnlineService.GetOnlinePlayers(roomId);
                    await Clients.Group(roomId).SendAsync("PlayerListUpdated", onlinePlayers);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
