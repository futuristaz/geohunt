using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using psi25_project.Models;

namespace psi25_project.Data
{
    public class GeoHuntContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public GeoHuntContext (DbContextOptions<GeoHuntContext> options)
            : base(options)
        {
        }

        // All domain tables here
        public DbSet<Location> Locations { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Guess> Guesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Game ↔ User (one-to-many)
            modelBuilder.Entity<Game>()
                .HasOne(g => g.User)
                .WithMany(u => u.Games)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Game ↔ Guess (one-to-many)
            modelBuilder.Entity<Game>()
                .HasMany(g => g.Guesses)
                .WithOne(gu => gu.Game)
                .HasForeignKey(gu => gu.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Guess ↔ Location (many-to-one)
            modelBuilder.Entity<Guess>()
                .HasOne(gu => gu.Location)
                .WithMany(l => l.Guesses)
                .HasForeignKey(gu => gu.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Unique coordinates
            modelBuilder.Entity<Location>()
                .HasIndex(l => new { l.Latitude, l.Longitude })
                .IsUnique();

            // --- Default timestamps
            modelBuilder.Entity<Location>()
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
}
