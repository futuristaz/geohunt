using Microsoft.AspNetCore.Mvc;
using Serilog;

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
        // Try multiple possible configuration paths
        // Expose only the client-side key to the browser.
        // The backend uses a separate key (`GoogleMaps:ServerApiKey`) for server-to-server calls.
        var googleMapsApiKey = _configuration["GoogleMaps:ClientApiKey"]
            ?? _configuration["GoogleMaps:ApiKey"]
            ?? _configuration["GoogleMaps__ClientApiKey"]
            ?? _configuration["GoogleMaps__ApiKey"]
            ?? Environment.GetEnvironmentVariable("GoogleMaps__ClientApiKey")
            ?? Environment.GetEnvironmentVariable("GoogleMaps__ApiKey");

        Log.Information("Config endpoint called. API Key present: {HasKey}", !string.IsNullOrEmpty(googleMapsApiKey));

        if (string.IsNullOrEmpty(googleMapsApiKey))
        {
            Log.Warning("Google Maps API key is not configured in any expected location");
            return StatusCode(500, new { error = "Google Maps API key is not configured" });
        }

        return Ok(new
        {
            googleMapsApiKey = googleMapsApiKey
        });
    }
}
