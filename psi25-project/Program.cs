using Microsoft.EntityFrameworkCore;
using psi25_project;
using psi25_project.Gateways;
using psi25_project.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Services ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<GoogleMapsGateway>();
builder.Services.AddControllers();
builder.Services.AddDbContext<GeoHuntContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<GeocodingService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

// Identity DbContext
//Creates a separate DbContext for Identity (ApplicationDbContext) to manage authentication and user accounts.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
//Configures cookie-based authentication for Identity.
//Sets the default paths for login and logout pages (useful if you later use MVC views).
//Ensures that unauthorized requests redirect properly.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
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

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
