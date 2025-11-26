using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using psi25_project.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AchievementController : ControllerBase
{
    private readonly IAchievementService _achievementService;
    private readonly ILogger<AchievementController> _logger;

    public AchievementController(IAchievementService achievementService, ILogger<AchievementController> logger)
    {
        _achievementService = achievementService;
        _logger = logger;
    }

    [HttpGet("available-achievements")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AchievementDto>>> GetAllAvailableAchievements()
    {
        try
        {
            var achievements = await _achievementService.GetActiveAchievementsAsync();

            if (achievements == null || achievements.Count == 0)
                return NoContent();

            return Ok(achievements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available achievements");
            return Problem("Failed to get available achievements");
        }
    }

    [HttpGet("achievements")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AchievementDto>>> GetUnlockedAchievementsForUser (Guid userId)
    {
        try
        {
            var achievements = await _achievementService.GetAchievementsForUserAsync(userId);

            if (achievements == null || achievements.Count == 0)
                return NoContent();
            
            return Ok(achievements);
        } catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unloced achievements");
            return Problem("Failed to get unlocked achievements");
        }
    }
}