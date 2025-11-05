using Microsoft.AspNetCore.Mvc;
using psi25_project.Services;
using System.Threading.Tasks;
using psi25_project.Data;

namespace psi25_project.Controllers
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
