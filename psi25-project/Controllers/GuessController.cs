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
    public async Task<ActionResult<Guess>> CreateGuess([FromBody] CreateGuessDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var game = await _context.Games.FindAsync(dto.GameId);
        if (game == null)
        {
            return NotFound("Game not found");
        }

        var location = await _context.Locations.FindAsync(dto.LocationId);
        if (location == null)
        {
            return NotFound("Location not found");
        }

        var guess = new Guess
        {
            GameId = dto.GameId,
            Game = game,
            LocationId = dto.LocationId,
            Location = location,
            RoundNumber = 0,
            GuessedAt = DateTime.UtcNow,
            GuessedLatitude = dto.GuessedLatitude,
            GuessedLongitude = dto.GuessedLongitude,
            DistanceKm = dto.DistanceKm,
            Score = dto.Score
        };

        var existingGuesses = await _context.Guesses.CountAsync(g => g.GameId == dto.GameId); // Async LINQ
        guess.RoundNumber = existingGuesses + 1;

        _context.Guesses.Add(guess);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateGuess), new { id = guess.Id }, new GuessResponseDto
        {
            Id = guess.Id,
            GameId = guess.GameId,
            LocationId = guess.LocationId,
            RoundNumber = guess.RoundNumber,
            GuessedLatitude = guess.GuessedLatitude,
            GuessedLongitude = guess.GuessedLongitude,
            DistanceKm = guess.DistanceKm,
            Score = guess.Score
        });
    }
}
