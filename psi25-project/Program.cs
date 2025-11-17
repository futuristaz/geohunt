using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using psi25_project;
using psi25_project.Gateways;
using psi25_project.Gateways.Interfaces;
using psi25_project.Services;
using psi25_project.Services.Interfaces;
using psi25_project.Repositories;
using psi25_project.Repositories.Interfaces;
using psi25_project.Models;
using psi25_project.Data;
using psi25_project.Middleware;
using Serilog;
using Polly;
using Polly.Extensions.Http;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/geohunt-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// ---------------- Services ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Register memory cache for caching Google Maps API responses
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 10_000; // Prevents unbounded memory growth
});

builder.Services.AddDbContext<GeoHuntContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IGuessRepository, GuessRepository>();
builder.Services.AddScoped<IGuessService, GuessService>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();

// ---------------- HTTP Client with Polly Resilience ----------------
builder.Services.AddHttpClient<IGoogleMapsGateway, GoogleMapsGateway>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

// ---------------- Identity Setup ----------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Optional: Password and user settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<GeoHuntContext>()
.AddDefaultTokenProviders();

builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost:5042")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});


// Configure login/logout cookie behavior
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Cookie.SameSite = SameSiteMode.Lax;

    options.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        else
        {
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        }
    };

    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = 403;
            return Task.CompletedTask;
        }
        else
        {
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

// ---------------- Build App ----------------
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

// Use global exception handler middleware (should be first to catch all exceptions)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// app.UseHttpsRedirection();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await RoleSeeder.SeedRoles(roleManager);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("/index.html"); // SPA routing in prod

try
{
    Log.Information("Starting GeoHunt application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Polly retry policy: retry on 5xx and network errors, but not on 4xx (client errors)
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 5xx and 408
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Log.Warning("Google Maps API call failed. Retry {RetryCount} after {Delay}s. Status: {StatusCode}",
                    retryCount, timespan.TotalSeconds, outcome.Result?.StatusCode);
            });
}

// Circuit breaker: open circuit after 5 consecutive failures, stay open for 30 seconds
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429 - consistent with retry policy
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, duration) =>
            {
                Log.Error("Google Maps API circuit breaker opened for {Duration}s due to consecutive failures", duration.TotalSeconds);
            },
            onReset: () =>
            {
                Log.Information("Google Maps API circuit breaker reset - resuming normal operations");
            });
}
