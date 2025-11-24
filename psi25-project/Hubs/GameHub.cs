using Microsoft.AspNetCore.SignalR;
using psi25_project.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psi25_project.Hubs
{
    public class GameHub : Hub
    {
        // Called when a player joins a room
        public async Task JoinRoom(string roomCode, string playerName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            await Clients.Group(roomCode).SendAsync("PlayerJoined", new
            {
                ConnectionId = Context.ConnectionId,
                PlayerName = playerName
            });
        }

        // Called when a player leaves a room
        public async Task LeaveRoom(string roomCode, string playerName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);

            await Clients.Group(roomCode).SendAsync("PlayerLeft", new
            {
                ConnectionId = Context.ConnectionId,
                PlayerName = playerName
            });
        }

        // Broadcast messages to a room
        public async Task SendMessageToRoom(string roomCode, string message)
        {
            await Clients.Group(roomCode).SendAsync("ReceiveMessage", message);
        }
    }
}
