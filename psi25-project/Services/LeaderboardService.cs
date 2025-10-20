using Microsoft.EntityFrameworkCore;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                .Select(g => new LeaderboardEntry
                {
                    Id = g.Id,
                    DistanceKm = g.DistanceKm,
                    GuessedAt = g.GuessedAt,
                    Score = g.Score
                })
                .ToListAsync();

            // Sort by score descending
            entries = entries.OrderByDescending(e => e.Score).ThenBy(e => e.DistanceKm).ToList();

            // Assign rank
            for (int i = 0; i < entries.Count; i++)
                entries[i].Rank = i + 1;

            return entries.Take(top).ToList();
        }
    }
}
