using Microsoft.AspNetCore.Mvc;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]

public class GuessController : ControllerBase
{
    private readonly GeoHuntContext _context;

    public GuessController(GeoHuntContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult> CreateGuess([FromBody] CreateGuessDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await using var tx = await _context.Database.BeginTransactionAsync();

        var game = await _context.Games.SingleOrDefaultAsync(g => g.Id == dto.GameId);
        if (game == null)
            return NotFound("Game not found");

        if (game.FinishedAt != null)
            return BadRequest("Game is already finished");

        var location = await _context.Locations.FindAsync(dto.LocationId);
        if (location == null)
            return NotFound("Location not found");

        // Use the authoritative current round from Game
        var roundNumber = game.CurrentRound;

        var guess = new Guess
        {
            GameId = dto.GameId,
            Game = game,
            LocationId = dto.LocationId,
            Location = location,
            RoundNumber = roundNumber,
            GuessedAt = DateTime.UtcNow,
            GuessedLatitude = dto.GuessedLatitude,
            GuessedLongitude = dto.GuessedLongitude,
            DistanceKm = dto.DistanceKm,
            Score = dto.Score
        };

        _context.Guesses.Add(guess);

        // Update game aggregate
        game.TotalScore += dto.Score;

        var isLastRound = roundNumber >= game.TotalRounds;
        if (isLastRound)
        {
            game.FinishedAt = DateTime.UtcNow;
        }
        else
        {
            game.CurrentRound = roundNumber + 1;
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        // Return a richer payload the client can act on
        return CreatedAtAction(nameof(CreateGuess), new { id = guess.Id }, new
        {
            guess = new GuessResponseDto
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
            },
            finished = isLastRound,
            currentRound = game.CurrentRound, // if finished, this will equal TotalRounds (or roundNumber+1 if you prefer)
            totalScore = game.TotalScore
        });
    }

    [HttpGet("{gameId}")]
    public async Task<ActionResult<IEnumerable<GuessResponseDto>>> GetGuessesForGame(Guid gameId)
    {
        var guesses = await _context.Guesses
                .Where(g => g.GameId == gameId)
                .Select(g => new GuessResponseDto
                {
                    Id = g.Id,
                    GameId = g.GameId,
                    LocationId = g.LocationId,
                    RoundNumber = g.RoundNumber,
                    GuessedLatitude = g.GuessedLatitude,
                    GuessedLongitude = g.GuessedLongitude,
                    DistanceKm = g.DistanceKm,
                    Score = g.Score,
                    // Add location data
                    ActualLatitude = g.Location.Latitude,
                    ActualLongitude = g.Location.Longitude
                })
                .ToListAsync();

        return Ok(guesses);
    }
}
