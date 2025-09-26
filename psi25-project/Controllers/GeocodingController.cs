using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GeocodingController : ControllerBase
{
    private readonly GoogleMapsGateway _mapsService;

    public GeocodingController(GoogleMapsGateway mapsService)
    {
        _mapsService = mapsService;
    }

    [HttpGet("distance")]
    public async Task<IActionResult> GetDistance(string address1, string address2)
    {
        try
        {
            var coords1 = await _mapsService.GetCoordinatesAsync(address1);
            var coords2 = await _mapsService.GetCoordinatesAsync(address2);

            var distance = DistanceCalculator.CalculateHaversineDistance(coords1, coords2, 2);

            return Ok(new { coords = new { first = new { lat = coords1.lat, lng = coords1.lng }, second = new { lat = coords2.lat, lng = coords2.lng } }, distance });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    [HttpGet("address")]
    public async Task<IActionResult> GetCoordinates(string address)
    {
        try
        {
            var coords = await _mapsService.GetCoordinatesAsync(address);
            return Ok(new { lat = coords.lat, lng = coords.lng });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error geocoding address: {ex.Message}");
        }
    }
}