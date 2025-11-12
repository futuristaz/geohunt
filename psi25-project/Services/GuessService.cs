using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psi25_project.Services
{
    public class GuessService : IGuessService
    {
        private readonly IGuessRepository _repository;

        public GuessService(IGuessRepository repository)
        {
            _repository = repository;
        }

        public async Task<(GuessResponseDto guess, bool finished, int currentRound, int totalScore)> CreateGuessAsync(CreateGuessDto dto)
        {
            var game = await _repository.GetGameByIdAsync(dto.GameId);
            if (game == null)
                throw new KeyNotFoundException("Game not found");

            if (game.FinishedAt != null)
                throw new InvalidOperationException("Game is already finished");

            var location = await _repository.GetLocationByIdAsync(dto.LocationId);
            if (location == null)
                throw new KeyNotFoundException("Location not found");

            var roundNumber = game.CurrentRound;

            var guess = new Guess
            {
                GameId = game.Id,
                Game = game,
                LocationId = location.Id,
                Location = location,
                RoundNumber = roundNumber,
                GuessedAt = DateTime.UtcNow,
                GuessedLatitude = dto.GuessedLatitude,
                GuessedLongitude = dto.GuessedLongitude,
                DistanceKm = dto.DistanceKm,
                Score = dto.Score
            };

            await _repository.AddGuessAsync(guess);

            // Update game
            game.TotalScore += dto.Score;

            var isLastRound = roundNumber >= game.TotalRounds;
            if (isLastRound)
                game.FinishedAt = DateTime.UtcNow;
            else
                game.CurrentRound = roundNumber + 1;

            await _repository.SaveGameAsync(game);

            var guessDto = new GuessResponseDto
            {
                Id = guess.Id,
                GameId = guess.GameId,
                LocationId = guess.LocationId,
                RoundNumber = guess.RoundNumber,
                GuessedLatitude = guess.GuessedLatitude,
                GuessedLongitude = guess.GuessedLongitude,
                DistanceKm = guess.DistanceKm,
                Score = guess.Score,
                ActualLatitude = location.Latitude,
                ActualLongitude = location.Longitude
            };

            return (guessDto, isLastRound, game.CurrentRound, game.TotalScore);
        }

        public async Task<List<GuessResponseDto>> GetGuessesForGameAsync(Guid gameId)
        {
            var guesses = await _repository.GetGuessesForGameAsync(gameId);
            var result = new List<GuessResponseDto>();

            foreach (var g in guesses)
            {
                result.Add(new GuessResponseDto
                {
                    Id = g.Id,
                    GameId = g.GameId,
                    LocationId = g.LocationId,
                    RoundNumber = g.RoundNumber,
                    GuessedLatitude = g.GuessedLatitude,
                    GuessedLongitude = g.GuessedLongitude,
                    DistanceKm = g.DistanceKm,
                    Score = g.Score,
                    ActualLatitude = g.Location.Latitude,
                    ActualLongitude = g.Location.Longitude
                });
            }

            return result;
        }
    }
}
