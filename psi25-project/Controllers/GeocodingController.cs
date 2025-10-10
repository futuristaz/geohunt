using Microsoft.AspNetCore.Mvc;
using psi25_project.Services;

[ApiController]
[Route("api/[controller]")]
public class GeocodingController : ControllerBase
{
    private readonly GeocodingService _geocodingService;

    public GeocodingController(GeocodingService geocodingService)
    {
        _geocodingService = geocodingService;
    }

    [HttpGet("valid_coords")]
    public async Task<IActionResult> GetValidCoordinates()
    {
        try
        {
            var (success, result) = await _geocodingService.GetValidCoordinatesAsync();

            if (success)
                return Ok(result);
            else
                return NotFound(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}