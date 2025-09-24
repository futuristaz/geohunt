using System.Diagnostics.Eventing.Reader;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<GoogleMapsGateway>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.MapGet("api/geocoding/distance", async (string address1, string address2, GoogleMapsGateway mapsService) =>
{
try
{
    var coords1 = await mapsService.GetCoordinatesAsync(address1);
    var coords2 = await mapsService.GetCoordinatesAsync(address2);

    var distance = DistanceCalculator.CalculateHaversineDistance(coords1, coords2, 2);

    return Results.Ok(new {coords = new {first = new {lat = coords1.lat, lng = coords1.lng}, second = new {lat = coords2.lat, lng = coords2.lng}}, distance });

    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error: {ex.Message}");
    }
});

app.MapGet("api/geocoding/address", async (string address, GoogleMapsGateway mapsService) =>
{
    try 
    {
        var coords = await mapsService.GetCoordinatesAsync(address);
        return Results.Ok(new { lat = coords.lat, lng = coords.lng });
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error geocoding address: {ex.Message}");
    }
});

app.Run();