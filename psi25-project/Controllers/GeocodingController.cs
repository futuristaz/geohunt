using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using psi25_project;

[ApiController]
[Route("api/[controller]")]
public class GeocodingController : ControllerBase
{
    private readonly GoogleMapsGateway _mapsService;

    public GeocodingController(GoogleMapsGateway mapsService)
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


    //--------------------------------------------------------------------------------------
    //FINAL
    //--------------------------------------------------------------------------------------
    [HttpGet("valid_coords")]
    public async Task<IActionResult> GetValidCoordinates(int maxTries = 1000)
    {
        try
        {
            string address = AddressProvider.GetRandomAddress();
            var coords = await _mapsService.GetCoordinatesAsync(address);

            double lat = coords.lat;
            double lng = coords.lng;

            int attempts = 0;

            StreetViewLocation? streetView = null;

            while (attempts < maxTries)
            {
                attempts++;

                (lat, lng) = CoordinateModifier.ModifyCoordinates(lat, lng);
                streetView = await _mapsService.GetStreetViewMetadataAsync(lat, lng);

                if (streetView != null)
                {
                    return Ok(new
                    {
                        address,
                        modifiedCoordinates = new { lat, lng },
                        panoID = streetView.PanoId,
                        attempts
                    });
                }
            }
            return NotFound(new
            {
                address,
                attempts,
                message = "No valid Street View found after retries"
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error processing request: {ex.Message}");
        }
    } 
    //--------------------------------------------------------------------------------------

}