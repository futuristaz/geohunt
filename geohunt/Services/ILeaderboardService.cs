using geohunt.Models.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace geohunt.Services
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardEntry>> GetTopLeaderboardAsync(int top = 20);
    }
}
