using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using psi25_project;
using psi25_project.Gateways;
using psi25_project.Services;
using psi25_project.Models; // <-- for ApplicationUser
using psi25_project.Data;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Services ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<GoogleMapsGateway>();
builder.Services.AddControllers();

// Register single unified DbContext (GeoHuntContext)
builder.Services.AddDbContext<GeoHuntContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Your domain services
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<GeocodingService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

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


// Configure login/logout cookie behavior
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
