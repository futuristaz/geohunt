using Microsoft.EntityFrameworkCore;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using psi25_project.Data;


namespace psi25_project.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly GeoHuntContext _context;

        public LeaderboardService(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<List<LeaderboardEntry>> GetTopLeaderboardAsync(int top = 20)
        {
            var entries = await _context.Guesses
                .Include(g => g.Game)
                .ThenInclude(game => game.User)
                .Select(g => new LeaderboardEntry
                {
                    Id = g.Id,
                    DistanceKm = g.DistanceKm,
                    GuessedAt = g.GuessedAt,
                    TotalScore = g.Score,
                    UserId = g.Game.User.Id,
                    Username = g.Game.User.UserName
                })
                .OrderByDescending(x => x.TotalScore)
                .Take(top)
                .ToListAsync();

            entries.Sort();

            for (int i = 0; i < entries.Count; i++)
                entries[i].Rank = i + 1;

            return entries;
        }
    }
}
