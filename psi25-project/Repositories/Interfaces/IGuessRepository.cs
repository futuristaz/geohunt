using psi25_project.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psi25_project.Repositories.Interfaces
{
    public interface IGuessRepository
    {
        Task<Game?> GetGameByIdAsync(Guid gameId);
        Task<Location?> GetLocationByIdAsync(int locationId);
        Task AddGuessAsync(Guess guess);
        Task SaveGameAsync(Game game);
        Task<List<Guess>> GetGuessesForGameAsync(Guid gameId);
    }
}
