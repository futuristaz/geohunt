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
        public DbSet<Achievement> Achievements { get; set; } = default!;
        public DbSet<UserAchievement> UserAchievements { get; set; } = default!;
        public DbSet<UserStats> UserStats { get; set; } = default!;

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

            ConfigureAchievements(modelBuilder);
            ConfigureUserAchievements(modelBuilder);
            ConfigureUserStats(modelBuilder);
        }

        private static void ConfigureAchievements(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Achievement>();

            entity.HasKey(a => a.Id);

            entity.Property(a => a.Code)
                .IsRequired()
                .HasMaxLength(64);

            entity.HasIndex(a => a.Code)
                .IsUnique();

            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(a => a.Description)
                .IsRequired()
                .HasMaxLength(512);

            // Store enum as int in DB
            entity.Property(a => a.Scope)
                .HasConversion<int>();

            entity.Property(a => a.IsActive)
                .HasDefaultValue(true);

            entity.HasData(
                new Achievement
                {
                    Id = 1,
                    Code = "FIRST_GUESS",
                    Name = "First Guess",
                    Description = "Make your first guess",
                    Scope = AchievementScope.Meta,
                    IsActive = true
                },
                new Achievement
                {
                    Id = 2,
                    Code = "BULLSEYE_100M",
                    Name = "Bullseye",
                    Description = "Guess within 100 m",
                    Scope = AchievementScope.Round,
                    IsActive = true
                },
                new Achievement
                {
                    Id = 3,
                    Code = "NEAR_1KM",
                    Name = "Near Enough",
                    Description = "Guess within 1 km",
                    Scope = AchievementScope.Round,
                    IsActive = true
                },
                new Achievement
                {
                    Id = 4,
                    Code = "SCORE_10K",
                    Name = "Five Digits",
                    Description = "Score 10,000+ points in a game",
                    Scope = AchievementScope.Game,
                    IsActive = true
                },
                new Achievement
                {
                    Id = 5,
                    Code = "CLEAN_SWEEP",
                    Name = "Clean Sweep",
                    Description = "All rounds in a game are <= 1 km distance",
                    Scope = AchievementScope.Game,
                    IsActive = true
                }
            );
        }

        private static void ConfigureUserAchievements(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<UserAchievement>();

            entity.HasKey(ua => ua.Id);

            // Relationship: UserAchievement → Achievement (many-to-one)
            entity.HasOne(ua => ua.Achievement)
                .WithMany()
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: UserAchievement → ApplicationUser (many-to-one)
            entity.HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent the same achievement being unlocked multiple times for the same user
            entity.HasIndex(ua => new { ua.UserId, ua.AchievementId })
                .IsUnique();

            // Optional: default value for UnlockedAt on DB side (you can also set it in code)
            entity.Property(ua => ua.UnlockedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }

        private static void ConfigureUserStats(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<UserStats>();

            // UserId is PK
            entity.HasKey(us => us.UserId);

            // 1:1 UserStats ↔ ApplicationUser
            entity.HasOne(us => us.User)
                .WithOne() // or .WithOne(u => u.Stats) if you add a Stats nav on ApplicationUser
                .HasForeignKey<UserStats>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reasonable defaults
            entity.Property(us => us.TotalGuesses)
                .HasDefaultValue(0);

            entity.Property(us => us.TotalGames)
                .HasDefaultValue(0);

            entity.Property(us => us.BestGameScore)
                .HasDefaultValue(0);

            entity.Property(us => us.CurrentStreakDays)
                .HasDefaultValue(0);
        }
    }
}
