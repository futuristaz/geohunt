using System.Diagnostics.Eventing.Reader;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<EventsRepository>();
builder.Services.AddHttpClient<GoogleMapsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/maps/distance", async (string address1, string address2, GoogleMapsService mapsService) =>
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

app.MapGet("/maps", async (string address, GoogleMapsService mapsService) =>
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

app.MapGet("/", async () =>
{
    await Task.Delay(1000);
    return "Hello, tester (delayed)";
});

app.MapGet("/problem", () => {
    return TypedResults.StatusCode(200);
});

app.MapGet("/greet/{name?}", (string name = "Guest", int age = 99) =>
{
    var text = $"Hello, {name}! You are {age} years old!";
    return text;
});

app.MapGet("/test1", () => {
    return new ComplexType
    {
        Id = 1,
        Name = "Name",
    };
});

app.MapPost("/testingpost", (ComplexType complexType) =>
{
    return $"id: {complexType.Id}, name: {complexType.Name}";
});

app.MapGet("/events", (EventsRepository repository) => repository.Get());

app.MapDelete("/", () => "This is a DELETE request!");
app.MapPut("/", () => "This is a PUT request!");
app.MapPatch("/", () => "This is a PATCH request!");

app.Run();

class ComplexType
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}

class EventsRepository
{
    public void Save(Event eventModel)
    {
        // Save the event
    }

    public Event[] Get()
    {
        return [
            new Event { Id = 1, Name = "Event 1" },
            new Event { Id = 2, Name = "Event 2" }
        ];
    }
}

class Event 
{
    public int Id { get; set; }
    public string Name { get; set; }
}