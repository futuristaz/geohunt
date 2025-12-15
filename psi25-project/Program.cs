using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using psi25_project.Gateways;
using psi25_project.Gateways.Interfaces;
using psi25_project.Services;
using psi25_project.Services.Interfaces;
using psi25_project.Repositories;
using psi25_project.Repositories.Interfaces;
using psi25_project.Models;
using psi25_project.Data;
using psi25_project.Utils;
using psi25_project.Models.Dtos;
using psi25_project.Middleware;
using psi25_project.Configuration;
using Serilog;
using psi25_project.Hubs;
using Npgsql;

Log.Logger = LoggingConfiguration.CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// ---------------- Services ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 10_000;
});

// Read connection string - Render provides DATABASE_URL
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Validate connection string exists
if (string.IsNullOrWhiteSpace(rawConnectionString))
{
    var errorMsg = "Database connection string is missing or empty.\n" +
                   "Checked: DATABASE_URL, ConnectionStrings__DefaultConnection environment variables.\n" +
                   "Please verify environment variables in Render Dashboard > Service > Environment tab.";
    Log.Fatal(errorMsg);
    throw new InvalidOperationException(errorMsg);
}

// DEBUG: Log the raw connection string length and content
Log.Information("DEBUG: Raw connection string length: {Length}", rawConnectionString.Length);
Log.Information("DEBUG: Raw connection string ends with: '{Ending}'",
    rawConnectionString.Length > 20 ? rawConnectionString[^20..] : rawConnectionString);

// Normalize Render's postgres:// to postgresql:// for compatibility
var normalizedConnectionString = rawConnectionString;
if (normalizedConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
    !normalizedConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    normalizedConnectionString = "postgresql://" + normalizedConnectionString.Substring("postgres://".Length);
    Log.Information("Normalized connection string from postgres:// to postgresql://");
}

// Build a safe Npgsql connection string (convert URI to key=value)
NpgsqlConnectionStringBuilder npgsqlBuilder;
try
{
    if (normalizedConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(normalizedConnectionString);
        var userInfoParts = uri.UserInfo.Split(':', 2);
        var username = userInfoParts.Length > 0 ? Uri.UnescapeDataString(userInfoParts[0]) : string.Empty;
        var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : string.Empty;

        npgsqlBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = username,
            Password = password
        };

        // Copy query params (if any) into the builder
        if (!string.IsNullOrEmpty(uri.Query))
        {
            var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in query)
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 2 && !string.IsNullOrWhiteSpace(kv[0]))
                {
                    npgsqlBuilder[kv[0]] = Uri.UnescapeDataString(kv[1]);
                }
            }
        }
    }
    else
    {
        // Already key=value style
        npgsqlBuilder = new NpgsqlConnectionStringBuilder(normalizedConnectionString);
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to parse database connection string.");
    throw;
}

// Enforce SSL for Render (and accept their cert)
npgsqlBuilder.SslMode = SslMode.Require;
npgsqlBuilder.TrustServerCertificate = true;

var finalConnectionString = npgsqlBuilder.ConnectionString;

// Validate connection string format
if (string.IsNullOrWhiteSpace(npgsqlBuilder.Host) || string.IsNullOrWhiteSpace(npgsqlBuilder.Database))
{
    Log.Fatal("Connection string appears malformed. Missing host or database.");
    Log.Fatal("Full connection string (with credentials): {FullConnectionString}", finalConnectionString);
}

// Log success (without exposing credentials)
var safeConnStr = finalConnectionString.Length > 20
    ? $"{finalConnectionString.Substring(0, 20)}...({finalConnectionString.Length} chars)"
    : "***";
Log.Information("Database connection configured: {SafeConnectionString}", safeConnStr);

// Log which source provided the connection string
var source = Environment.GetEnvironmentVariable("DATABASE_URL") != null ? "DATABASE_URL" :
             Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") != null ? "ConnectionStrings__DefaultConnection" :
             "Configuration";
Log.Information("Connection string source: {Source}", source);

builder.Services.AddDbContext<GeoHuntContext>(options =>
    options.UseNpgsql(finalConnectionString));

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
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IGuessService, GuessService>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddSingleton<IRoomOnlineService, RoomOnlineService>();
builder.Services.AddScoped<IMultiplayerGameRepository, MultiplayerGameRepository>();
builder.Services.AddScoped<IMultiplayerGameService, MultiplayerGameService>();

builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IUserStatsRepository, UserStatsRepository>();
builder.Services.AddScoped<IGuessRepository, GuessRepository>();
builder.Services.AddSingleton<ObjectValidator<LocationDto>>();

builder.Services.AddHttpClient<IGoogleMapsGateway, GoogleMapsGateway>()
    .AddPolicyHandler(HttpPolicyConfiguration.GetRetryPolicy())
    .AddPolicyHandler(HttpPolicyConfiguration.GetCircuitBreakerPolicy());

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<GeoHuntContext>()
.AddDefaultTokenProviders();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        var allowedOrigins = new List<string> { "http://localhost:5042" }; // React dev server

        // Add production origin from environment variable
        var productionOrigin = builder.Configuration["PRODUCTION_ORIGIN"];
        if (!string.IsNullOrEmpty(productionOrigin))
        {
            allowedOrigins.Add(productionOrigin);
        }

        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.SameSite = SameSiteMode.Lax;

    // Production-specific cookie settings
    if (!builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }

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

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// SignalR
builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// ---------------- Build App ----------------
var app = builder.Build();

// ---------------- Middleware & Seeding ----------------
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

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GeoHuntContext>();

    // Validate database connection before attempting migration
    try
    {
        Log.Information("Testing database connection...");
        Log.Information("Attempting to open database connection...");

        // Test the connection without disposing it (EF Core will reuse it)
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        Log.Information("Database connection opened successfully");
        connection.Close();
        Log.Information("Database connection successful");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Failed to connect to database. Error: {ErrorMessage}", ex.Message);
        Log.Fatal("Exception type: {ExceptionType}", ex.GetType().FullName);

        if (ex.InnerException != null)
        {
            Log.Fatal("Inner exception: {InnerExceptionType} - {InnerMessage}",
                ex.InnerException.GetType().FullName, ex.InnerException.Message);
        }

        // Log additional details for PostgreSQL specific exceptions
        if (ex is Npgsql.NpgsqlException npgsqlEx)
        {
            Log.Fatal("PostgreSQL error code: {ErrorCode}", npgsqlEx.ErrorCode);
            Log.Fatal("PostgreSQL SQL state: {SqlState}", npgsqlEx.SqlState);
        }

        throw;
    }

    await context.Database.MigrateAsync();
    await AchievementSeeder.SeedAchievements(context);

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    await RoleSeeder.SeedRoles(roleManager);
}

// ---------------- Routing ----------------
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

// ---------------- Map Endpoints ----------------
app.MapControllers();
app.MapHub<RoomHub>("/roomHub");
app.MapHub<GameHub>("/gameHub");
app.MapFallbackToFile("/index.html");

// ---------------- Run ----------------
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

public partial class Program { }
