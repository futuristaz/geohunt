using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psi25_project.Repositories
{
    public class GuessRepository : IGuessRepository
    {
        private readonly GeoHuntContext _context;

        public GuessRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<Game?> GetGameByIdAsync(Guid gameId)
        {
            return await _context.Games.FindAsync(gameId);
        }

        public async Task<Location?> GetLocationByIdAsync(int locationId)
        {
            return await _context.Locations.FindAsync(locationId);
        }

        public async Task AddGuessAsync(Guess guess)
        {
            await _context.Guesses.AddAsync(guess);
        }

        public async Task SaveGameAsync(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Guess>> GetGuessesForGameAsync(Guid gameId)
        {
            return await _context.Guesses
                .Include(g => g.Location)
                .Where(g => g.GameId == gameId)
                .ToListAsync();
        }
    }
}
