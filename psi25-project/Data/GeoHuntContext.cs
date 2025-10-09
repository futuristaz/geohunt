using Microsoft.EntityFrameworkCore;
using psi25_project.Models;

public class GeoHuntContext : DbContext
{
    public GeoHuntContext(DbContextOptions<GeoHuntContext> options) : base(options) { }

    public DbSet<psi25_project.Models.Location> Locations { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Guess> Guesses { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- User ↔ Game (one-to-many) ---
        modelBuilder.Entity<User>()
            .HasMany(u => u.Games)
            .WithOne(g => g.User)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade); // deletes user's games if user deleted

        // --- Game ↔ Guess (one-to-many) ---
        modelBuilder.Entity<Game>()
            .HasMany(g => g.Guesses)
            .WithOne(gu => gu.Game)
            .HasForeignKey(gu => gu.GameId)
            .OnDelete(DeleteBehavior.Cascade); // deletes game's guesses if game deleted

        // --- Guess ↔ Location (many-to-one) ---
        modelBuilder.Entity<Guess>()
            .HasOne(gu => gu.Location)
            .WithMany(l => l.Guesses)
            .HasForeignKey(gu => gu.LocationId)
            .OnDelete(DeleteBehavior.Restrict); // prevents deleting location if it has guesses

        modelBuilder.Entity<psi25_project.Models.Location>()
            .HasIndex(l => new { l.Latitude, l.Longitude })
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<psi25_project.Models.Location>()
            .Property(l => l.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Guess>()
            .Property(g => g.GuessedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Game>()
            .Property(g => g.StartedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}