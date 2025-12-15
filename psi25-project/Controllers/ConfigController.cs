using Microsoft.AspNetCore.Mvc;

namespace psi25_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Returns public configuration values that are safe to expose to the frontend
    /// </summary>
    [HttpGet("public")]
    public IActionResult GetPublicConfig()
    {
        var googleMapsApiKey = _configuration["GoogleMaps__ApiKey"];

        if (string.IsNullOrEmpty(googleMapsApiKey))
        {
            return StatusCode(500, new { error = "Google Maps API key is not configured" });
        }

        return Ok(new
        {
            googleMapsApiKey = googleMapsApiKey
        });
    }
}
