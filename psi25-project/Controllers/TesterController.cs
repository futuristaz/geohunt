//**************************************************************
//This controller used for testing, later will be deleted
//**************************************************************

using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using psi25_project;

[ApiController]
[Route("api/[controller]")]
public class TesterController : ControllerBase
{
    private readonly GoogleMapsGateway _mapsService;

    public TesterController(GoogleMapsGateway mapsService)
    {
        _mapsService = mapsService;
    }

    // --------------------------------------------------------------------------------------
    //Method to get coordinates(double; double) from address from <PickAddress.cs>
    //And modify them using <ModifyCoordinates.cs>
    // --------------------------------------------------------------------------------------
    [HttpGet("address")]
    public async Task<IActionResult> GetCoordinates()
    {
        try
        {
            string address = AddressProvider.GetRandomAddress();

            var coords = await _mapsService.GetCoordinatesAsync(address);
            var modified = CoordinateModifier.ModifyCoordinates(coords.lat, coords.lng);

            return Ok(new
            {
                address,
                original = new { lat = coords.lat, lng = coords.lng },
                modified = new { lat = modified.newLat, lng = modified.newLng }
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error geocoding address: {ex.Message}");
        }
    }
    // --------------------------------------------------------------------------------------


    // --------------------------------------------------------------------------------------
    //Method to verify if given coordinates(doulbe, double) has a streetview
    // --------------------------------------------------------------------------------------
    [HttpGet("validate")]
    public async Task<IActionResult> GetClosestStreetView(double lat, double lng)
    {
        var result = await _mapsService.GetStreetViewMetadataAsync(lat, lng);

        if (result == null)
            return NotFound(new { message = "No Street View found nearby" });

        return Ok(result);
    }
    // --------------------------------------------------------------------------------------
}