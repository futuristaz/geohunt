using System.Diagnostics.Eventing.Reader;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<EventsRepository>();
builder.Services.AddHttpClient<GoogleStreetViewService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

// ----------------------------------------------------------------------------------------------------------------
app.MapGet("/streetview", async (double lat, double lng, GoogleStreetViewService streetViewService) =>
{
    try
    {
        var imageBytes = await streetViewService.GetStreetViewAsync(lat, lng);
        return Results.File(imageBytes, "image/jpeg");
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error retrieving street view: {ex.Message}");
    }
});
// ----------------------------------------------------------------------------------------------------------------
app.UseStaticFiles();
// ----------------------------------------------------------------------------------------------------------------

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