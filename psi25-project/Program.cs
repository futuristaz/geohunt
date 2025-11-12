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

var builder = WebApplication.CreateBuilder(args);

// ---------------- Services ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<GoogleMapsGateway>();
builder.Services.AddControllers();

builder.Services.AddDbContext<GeoHuntContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddHttpClient<IGoogleMapsGateway, GoogleMapsGateway>();

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


app.Run();
