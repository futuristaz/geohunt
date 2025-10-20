using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using psi25_project.Models.Dtos;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace psi25_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly GeoHuntContext _context;

        public LeaderboardController(GeoHuntContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetLeaderboard()
        {
            var guesses = await _context.Guesses
                .Select(g => new LeaderboardEntry
                {
                    Id = g.Id,
                    DistanceKm = g.DistanceKm,
                    GuessedAt = g.GuessedAt,
                    Score = g.Score
                })
                .ToListAsync();

            guesses.Sort();

            for (int i = 0; i < guesses.Count; i++)
                guesses[i].Rank = i + 1;

            return Ok(guesses.Take(20));
        }
    }
}
