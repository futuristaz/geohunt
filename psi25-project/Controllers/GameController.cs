using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Data;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GeoHuntContext _context;

    public GameController(GeoHuntContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult> CreateGame([FromBody] CreateGameDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var game = new Game
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            User = user,
            TotalScore = 0, 
            StartedAt = DateTime.UtcNow,
            FinishedAt = null
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateGame), new { id = game.Id }, new GameResponseDto
        {
            Id = game.Id,
            UserId = game.UserId,
            TotalScore = game.TotalScore,
            StartedAt = game.StartedAt,
            FinishedAt = game.FinishedAt
        });

    }

    [HttpPatch("{id}/finish")]
    public async Task<ActionResult<Game>> FinishGame(Guid id)
    {
        var game = await _context.Games.FindAsync(id);
        if (game == null)
        {
            return NotFound("Game not found");
        }

        if (game.FinishedAt != null)
        {
            return BadRequest("Game is already finished");
        }

        game.FinishedAt = DateTime.UtcNow;
        _context.Games.Update(game);
        await _context.SaveChangesAsync();

        return Ok(game);
    }

    [HttpPatch("{id}/score")]
    public async Task<ActionResult<Game>> UpdateScore(Guid id, [FromBody] int score)
    {
        var game = await _context.Games.FindAsync(id);
        if (game == null)
        {
            return NotFound("Game not found");
        }

        game.TotalScore += score;
        _context.Games.Update(game);
        await _context.SaveChangesAsync();

        return Ok(game);
    }

    [HttpGet("{id}/total-score")]
    public async Task<ActionResult<int>> GetTotalScore(Guid id)
    {
        var totalScore = await _context.Guesses
            .Where(g => g.GameId == id)
            .SumAsync(g => (int?)g.Score) ?? 0;
        return Ok(totalScore);
    }
}