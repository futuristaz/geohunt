using Microsoft.AspNetCore.Mvc;
using geohunt.Services;
using System.Threading.Tasks;

namespace geohunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<ActionResult> GetLeaderboard()
        {
            var leaderboard = await _leaderboardService.GetTopLeaderboardAsync();
            return Ok(leaderboard);
        }
    }
}
