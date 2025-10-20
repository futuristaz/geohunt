using psi25_project.Models.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psi25_project.Services
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardEntry>> GetTopLeaderboardAsync(int top = 20);
    }
}
