using psi25_project.Models;
using psi25_project.Models.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psi25_project.Repositories.Interfaces
{
    public interface ILeaderboardRepository
    {
        Task<List<LeaderboardEntry>> GetTopGuessesAsync(int top = 20);
        Task<List<LeaderboardEntry>> GetTopPlayersAsync(int top = 20);
    }
}
