using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services.Interfaces;

namespace psi25_project.Services
{
    public class GuessService : IGuessService
    {
        private readonly IGuessRepository _guessRepository;
        private readonly IGameRepository _gameRepository;
        private readonly ILocationRepository _locationRepository;

        public GuessService(
            IGuessRepository guessRepository,
            IGameRepository gameRepository,
            ILocationRepository locationRepository)
        {
            _guessRepository = guessRepository;
            _gameRepository = gameRepository;
            _locationRepository = locationRepository;
        }

        public async Task<(GuessResponseDto guess, bool finished, int currentRound, int totalScore)>
            CreateGuessAsync(CreateGuessDto dto)
        {
            var game = await _gameRepository.GetByIdAsync(dto.GameId)
                       ?? throw new KeyNotFoundException("Game not found");

            if (game.FinishedAt != null)
                throw new InvalidOperationException("Game is already finished");

            var location = await _locationRepository.GetByIdAsync(dto.LocationId)
                           ?? throw new KeyNotFoundException("Location not found");

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

            await _guessRepository.AddAsync(guess);

            UpdateGameProgress(game, dto.Score);

            await _gameRepository.UpdateAsync(game);

            var response = MapToDto(guess, location);

            return (response, game.FinishedAt != null, game.CurrentRound, game.TotalScore);
        }

        public async Task<List<GuessResponseDto>> GetGuessesForGameAsync(Guid gameId)
        {
            var guesses = await _guessRepository.GetGuessesByGameAsync(gameId);
            return guesses.Select(g => MapToDto(g, g.Location)).ToList();
        }

        private static GuessResponseDto MapToDto(Guess g, Location location)
        {
            return new GuessResponseDto
            {
                Id = g.Id,
                GameId = g.GameId,
                LocationId = g.LocationId,
                RoundNumber = g.RoundNumber,
                GuessedLatitude = g.GuessedLatitude,
                GuessedLongitude = g.GuessedLongitude,
                DistanceKm = g.DistanceKm,
                Score = g.Score,
                ActualLatitude = location.Latitude,
                ActualLongitude = location.Longitude
            };
        }

        private static void UpdateGameProgress(Game game, int score)
        {
            game.TotalScore += score;

            if (game.CurrentRound >= game.TotalRounds)
                game.FinishedAt = DateTime.UtcNow;
            else
                game.CurrentRound++;
        }
    }
}
