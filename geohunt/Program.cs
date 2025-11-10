using System.Diagnostics.Eventing.Reader;
using Microsoft.EntityFrameworkCore;
using geohunt;
using geohunt.Gateways;
using geohunt.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<GoogleMapsGateway>();
builder.Services.AddControllers();
builder.Services.AddDbContext<GeoHuntContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<GeocodingService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

app.UseStaticFiles();
app.MapControllers();

app.Run();