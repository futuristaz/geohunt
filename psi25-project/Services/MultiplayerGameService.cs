using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services.Interfaces;
using psi25_project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psi25_project.Services
{
    public class MultiplayerGameService : IMultiplayerGameService
    {
        private readonly IMultiplayerGameRepository _gameRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IRoomRepository _roomRepository;

        public MultiplayerGameService(
            IMultiplayerGameRepository gameRepository,
            IPlayerRepository playerRepository,
            IRoomRepository roomRepository)
        {
            _gameRepository = gameRepository;
            _playerRepository = playerRepository;
            _roomRepository = roomRepository;
        }

        public async Task<MultiplayerGameDto> StartGameAsync(Guid roomId)
        {
            var room = await _roomRepository.GetRoomByIdAsync(roomId);
            if (room == null) throw new KeyNotFoundException("Room not found");

            if (!room.Players.All(p => p.IsReady))
                throw new InvalidOperationException("Not all players are ready");

            var coords = GenerateRandomCoordinates();

            var game = new MultiplayerGame
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                StartedAt = DateTime.UtcNow,
                TotalRounds = room.TotalRounds,
                CurrentRound = 1,
                State = GameState.InProgress,
                RoundLatitude = coords.Lat,
                RoundLongitude = coords.Lng,
                Players = room.Players.Select(p => new MultiplayerPlayer
                {
                    Id = Guid.NewGuid(),
                    PlayerId = p.Id,
                    Score = 0,
                    IsReady = false,
                    Finished = false
                }).ToList()
            };

            await _gameRepository.AddAsync(game);

            // Update room state
            room.Status = RoomStatus.InGame;
            await _roomRepository.UpdateRoomAsync(room);

            return MapToDto(game);
        }

        public async Task<RoundResultDto> SubmitGuessAsync(Guid playerId, double latitude, double longitude)
        {
            var player = await _playerRepository.GetPlayerByIdAsync(playerId);
            if (player?.RoomId == null) throw new KeyNotFoundException("Player or room not found");

            var game = await _gameRepository.GetByRoomIdAsync(player.RoomId.Value);
            if (game == null) throw new InvalidOperationException("No active game found");

            var mp = game.Players.FirstOrDefault(p => p.PlayerId == playerId);
            if (mp == null) throw new KeyNotFoundException("Player not in game");

            mp.LastGuessLatitude = latitude;
            mp.LastGuessLongitude = longitude;
            mp.Finished = true;

            // Calculate distance using DistanceCalculator
            var distanceKm = DistanceCalculator.CalculateHaversineDistance(
                new GeocodeResultDto { Lat = latitude, Lng = longitude },
                new GeocodeResultDto { Lat = game.RoundLatitude ?? 0, Lng = game.RoundLongitude ?? 0 },
                2);

            mp.DistanceMeters = distanceKm * 1000; // convert km to meters

            // Calculate score using ScoreCalculator
            mp.Score += ScoreCalculator.CalculateGeoScore(distanceKm);

            await _gameRepository.UpdateAsync(game);

            var allFinished = game.Players.All(p => p.Finished);

            return new RoundResultDto
            {
                PlayerId = playerId,
                Score = mp.Score,
                DistanceMeters = mp.DistanceMeters ?? 0,
                RoundFinished = allFinished
            };
        }

        // --- Next Round ---
        public async Task<MultiplayerGameDto> NextRoundAsync(Guid gameId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null) throw new KeyNotFoundException("Game not found");

            if (game.CurrentRound >= game.TotalRounds)
                throw new InvalidOperationException("All rounds already completed");

            game.CurrentRound++;
            var coords = GenerateRandomCoordinates();
            game.RoundLatitude = coords.Lat;
            game.RoundLongitude = coords.Lng;

            foreach (var mp in game.Players)
            {
                mp.Finished = false;
                mp.LastGuessLatitude = null;
                mp.LastGuessLongitude = null;
                mp.DistanceMeters = null;
            }

            await _gameRepository.UpdateAsync(game);
            return MapToDto(game);
        }

        // --- End Game ---
        public async Task<MultiplayerGameDto> EndGameAsync(Guid gameId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null) throw new KeyNotFoundException("Game not found");

            game.FinishedAt = DateTime.UtcNow;
            game.State = GameState.Finished;

            var room = await _roomRepository.GetRoomByIdAsync(game.RoomId);
            if (room != null)
            {
                room.Status = RoomStatus.Lobby;
                
                foreach (var player in room.Players)
                {
                    player.IsReady = false;
                }
                
                await _roomRepository.UpdateRoomAsync(room);
            }

            await _gameRepository.UpdateAsync(game);
            return MapToDto(game);
        }

        // --- Past Games ---
        public async Task<List<GameResultDto>> GetPastGamesForRoomAsync(Guid roomId)
        {
            var games = await _gameRepository.GetPastGamesForRoomAsync(roomId);
            return games.Select(g => new GameResultDto
            {
                GameId = g.Id,
                StartedAt = g.StartedAt,
                FinishedAt = g.FinishedAt,
                PlayerScores = g.Players.Select(p => new PlayerScoreDto
                {
                    PlayerId = p.PlayerId,
                    DisplayName = p.Player.DisplayName,
                    Score = p.Score
                }).ToList()
            }).ToList();
        }

        public async Task<MultiplayerGame?> GetCurrentGameForRoomAsync(Guid roomId)
        {
            return await _gameRepository.GetByRoomIdAsync(roomId);
        }

        public async Task<MultiplayerPlayer?> GetCurrentGameForPlayerAsync(Guid playerId)
        {
            // Get player entity
            var player = await _playerRepository.GetPlayerByIdAsync(playerId);
            if (player == null || !player.RoomId.HasValue) return null;

            var roomId = player.RoomId.Value;

            // Get the current active game for the room
            var game = await _gameRepository.GetByRoomIdAsync(roomId);
            if (game == null) return null;

            // Return the multiplayer player object from the current game
            return game.Players.FirstOrDefault(p => p.PlayerId == playerId);
        }


        // --- Helpers ---
        private static MultiplayerGameDto MapToDto(MultiplayerGame game) => new()
        {
            GameId = game.Id,
            RoomId = game.RoomId,
            CurrentRound = game.CurrentRound,
            TotalRounds = game.TotalRounds,
            RoundLatitude = game.RoundLatitude,
            RoundLongitude = game.RoundLongitude,
            Players = game.Players.Select(p => new MultiplayerPlayerDto
            {
                PlayerId = p.PlayerId,
                DisplayName = p.Player.DisplayName,
                Score = p.Score,
                Finished = p.Finished,
                LastGuessLatitude = p.LastGuessLatitude,
                LastGuessLongitude = p.LastGuessLongitude,
                DistanceMeters = p.DistanceMeters
            }).ToList()
        };

        private static GeocodeResultDto GenerateRandomCoordinates()
        {
            var lat = 4.684954529774102;
            var lng = -74.53974505304815;
            return new GeocodeResultDto { Lat = lat, Lng = lng };
        }
    }
}
