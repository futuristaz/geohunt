using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using psi25_project.Services.Interfaces;
using psi25_project.Exceptions;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class GeocodingController : ControllerBase
{
    private readonly IGeocodingService _geocodingService;

    public GeocodingController(IGeocodingService geocodingService)
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
        catch (GoogleMapsApiException ex)
        {
            // Treat upstream Google Maps failures as service-unavailable so the frontend can show a retry message.
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                code = "MAPS_UNAVAILABLE",
                endpoint = ex.Endpoint,
                errorCode = ex.ErrorCode,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
