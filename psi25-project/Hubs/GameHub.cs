using Microsoft.AspNetCore.SignalR;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace psi25_project.Hubs
{
    public class GameHub : Hub
    {
        private readonly IMultiplayerGameService _gameService;

        public GameHub(IMultiplayerGameService gameService)
        {
            _gameService = gameService;
        }

        // --- Start Game ---
        public async Task StartGame(Guid roomId)
        {
            // Start game only if all players are ready
            var gameDto = await _gameService.StartGameAsync(roomId);

            // Add all connections in this room to a SignalR group
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

            // Broadcast round start to all players in the room
            await Clients.Group(roomId.ToString())
                .SendAsync("RoundStarted", new
                {
                    gameDto.CurrentRound,
                    gameDto.TotalRounds,
                    gameDto.RoundLatitude,
                    gameDto.RoundLongitude
                });

            // Broadcast initial player scores
            await Clients.Group(roomId.ToString())
                .SendAsync("GameStateUpdated", gameDto);
        }

        // --- Submit Guess ---
        public async Task SubmitGuess(Guid playerId, double latitude, double longitude)
        {
            var result = await _gameService.SubmitGuessAsync(playerId, latitude, longitude);

            // Send updated player round result to everyone in the room
            var player = await _gameService.GetCurrentGameForPlayerAsync(playerId);
            if (player == null) return;

            var roomId = player.Game.RoomId;

            await Clients.Group(roomId.ToString())
                .SendAsync("RoundResult", result);

            // If all players finished the round, automatically start next round or end game
            var game = await _gameService.GetCurrentGameForRoomAsync(roomId);
            if (game == null) return;

            if (game.Players.All(p => p.Finished))
            {
                if (game.CurrentRound < game.TotalRounds)
                {
                    // Start next round
                    var nextRound = await _gameService.NextRoundAsync(game.Id);
                    await Clients.Group(roomId.ToString())
                        .SendAsync("RoundStarted", new
                        {
                            nextRound.CurrentRound,
                            nextRound.TotalRounds,
                            nextRound.RoundLatitude,
                            nextRound.RoundLongitude
                        });

                    await Clients.Group(roomId.ToString())
                        .SendAsync("GameStateUpdated", nextRound);
                }
                else
                {
                    // End game
                    var finishedGame = await _gameService.EndGameAsync(game.Id);
                    await Clients.Group(roomId.ToString())
                        .SendAsync("GameFinished", finishedGame);

                    // Optionally: send leaderboard for the lobby
                    var pastGames = await _gameService.GetPastGamesForRoomAsync(roomId);
                    await Clients.Group(roomId.ToString())
                        .SendAsync("PastGamesUpdated", pastGames);
                }
            }
        }

        // --- Get current game state for a player ---
        public async Task GetCurrentGame(Guid roomId)
        {
            var game = await _gameService.GetCurrentGameForRoomAsync(roomId);
            if (game == null) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

            await Clients.Caller.SendAsync("GameStateUpdated", game);
        }

        // --- Optional: handle disconnect ---
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Could remove player from any ongoing games if needed
            await base.OnDisconnectedAsync(exception);
        }
    }
}
