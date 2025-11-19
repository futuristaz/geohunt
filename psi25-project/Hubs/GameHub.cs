using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace psi25_project.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinRoom(string roomCode, string playerName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            await Clients.Group(roomCode).SendAsync("PlayerJoined", playerName);
        }
    }
}
