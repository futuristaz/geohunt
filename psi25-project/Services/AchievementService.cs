using psi25_project.Models;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services.Interfaces;

namespace psi25_project.Services;

public class AchievementService : IAchievementService
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IUserStatsRepository _userStatsRepository;
    private readonly IGuessRepository _guessRepository;

    private static readonly string[] RoundSubmittedCodes = new[] 
    {
        AchievementCodes.FirstGuess,
        AchievementCodes.Bullseye100m,
        AchievementCodes.Near1km
    };
    private static readonly string[] GameFinishedCodes = new[]
    {
        AchievementCodes.Score10k,
        AchievementCodes.CleanSweep,
        AchievementCodes.TheMarahoner,
        AchievementCodes.StreakMaster,
        AchievementCodes.LateNightPlayer
    };

    public AchievementService(
        IAchievementRepository achievementRepository,
        IUserStatsRepository userStatsRepository,
        IGuessRepository guessRepository)
    {
        _achievementRepository = achievementRepository;
        _userStatsRepository = userStatsRepository;
        _guessRepository = guessRepository;
    }

    public async Task<IReadOnlyList<UserAchievement>> OnRoundSubmittedAsync(Guid userId, Guid gameId, int roundNumber, double distanceKm, int score)
    {
        // Load only relevant achievements
        var catalog = await _achievementRepository.GetActiveByCodesAsync(RoundSubmittedCodes);

        // Get / create user stats
        var stats = await _userStatsRepository.GetOrCreateAsync(userId);

        // is this user's first guess overall
        var isFirstGuess = stats.TotalGuesses == 0;
        stats.TotalGuesses += 1;

        await _userStatsRepository.UpdateAsync(stats);

        // decide which codes to unlock for this round
        var toUnlockCodes = new List<string>();
        if (isFirstGuess) toUnlockCodes.Add(AchievementCodes.FirstGuess);
        if (distanceKm <= 0.1) toUnlockCodes.Add(AchievementCodes.Bullseye100m);
        if (distanceKm <= 1.0) toUnlockCodes.Add(AchievementCodes.Near1km);

        if (toUnlockCodes.Count == 0) return Array.Empty<UserAchievement>();

        // map codes -> achievement ids
        var targetAchievements = catalog
            .Where(a => toUnlockCodes.Contains(a.Code))
            .ToList();

        if (targetAchievements.Count == 0) return Array.Empty<UserAchievement>();

        // filter out already unlocked achievements
        var alreadyUnlocked = await _achievementRepository.GetUnlockedAsync(
            userId,
            targetAchievements.Select(a => a.Id)
        );

        var alreadyIds = alreadyUnlocked
            .Select(ua => ua.AchievementId)
            .ToHashSet();

        var newUnlocks = targetAchievements
            .Where(a => !alreadyIds.Contains(a.Id))
            .Select(a => new UserAchievement
            {
                UserId = userId,
                AchievementId = a.Id,
                // Achievement = a,
                UnlockedAt = DateTime.UtcNow
            })
            .ToList();

        if (newUnlocks.Count == 0)
            return Array.Empty<UserAchievement>();

        await _achievementRepository.AddNewlyUnlockedAchievementsAsync(newUnlocks);

        var unlockedWithAchievements = await _achievementRepository.GetUnlockedAsync(
            userId,
            newUnlocks.Select(u => u.AchievementId)
        );

        return unlockedWithAchievements;
    }

    public async Task<IReadOnlyList<UserAchievement>> OnGameFinishedAsync(Guid userId, Guid gameId, int totalScore, int totalRounds)
    {
        // 1) Load relevant achievements
        var catalog = await _achievementRepository.GetActiveByCodesAsync(GameFinishedCodes);

        // Get / create user stats
        var stats = await _userStatsRepository.GetOrCreateAsync(userId);
        stats.TotalGames += 1;
        if (totalScore > stats.BestGameScore) stats.BestGameScore = totalScore;

        var today = DateTime.UtcNow.Date;
        var lastPlayed = stats.LastPlayedDateUtc?.Date;

        if (lastPlayed == null) stats.CurrentStreakDays = 1;
        else if (lastPlayed == today) { /* do nothing */ }
        else if (lastPlayed == today.AddDays(-1)) stats.CurrentStreakDays += 1;
        else stats.CurrentStreakDays = 1;

        stats.LastPlayedDateUtc = DateTime.UtcNow;
        if (stats.CurrentStreakDays > stats.LongestStreakDays) stats.LongestStreakDays = stats.CurrentStreakDays; 

        await _userStatsRepository.UpdateAsync(stats);

        // 2) Evaluate conditions
        var toUnlockCodes = new List<string>();

        // Score 10k
        if (totalScore >= 10_000)
            toUnlockCodes.Add(AchievementCodes.Score10k);

        // Clean sweep: all guesses in this game within 1km
        var guesses = await _guessRepository.GetGuessesByGameAsync(gameId);
        if (guesses.Count() == totalRounds && guesses.All(g => g.DistanceKm <= 1.0))
        {
            toUnlockCodes.Add(AchievementCodes.CleanSweep);
        }

        // The Marathoner
        if (stats.TotalGames == 100) toUnlockCodes.Add(AchievementCodes.TheMarahoner);

        // Streak Master
        if (stats.CurrentStreakDays == 30) toUnlockCodes.Add(AchievementCodes.StreakMaster);

        // Late Night Player
        if (DateTime.UtcNow.Hour >= 0 && DateTime.UtcNow.Hour <= 6) toUnlockCodes.Add(AchievementCodes.LateNightPlayer);

        if (toUnlockCodes.Count == 0)
            return Array.Empty<UserAchievement>();

        // 3) Map codes -> Achievement IDs
        var targetAchievements = catalog
            .Where(a => toUnlockCodes.Contains(a.Code))
            .ToList();

        if (targetAchievements.Count == 0)
            return Array.Empty<UserAchievement>();

        // 4) Filter out already unlocked
        var alreadyUnlocked = await _achievementRepository.GetUnlockedAsync(
            userId,
            targetAchievements.Select(a => a.Id)
        );

        var alreadyIds = alreadyUnlocked
            .Select(ua => ua.AchievementId)
            .ToHashSet();

        var newUnlocks = targetAchievements
            .Where(a => !alreadyIds.Contains(a.Id))
            .Select(a => new UserAchievement
            {
                UserId = userId,
                AchievementId = a.Id,
                // Achievement = a,
                UnlockedAt = DateTime.UtcNow
            })
            .ToList();

        if (newUnlocks.Count == 0)
            return Array.Empty<UserAchievement>();

        await _achievementRepository.AddNewlyUnlockedAchievementsAsync(newUnlocks);

        var unlockedWithAchievements = await _achievementRepository.GetUnlockedAsync(
            userId,
            newUnlocks.Select(u => u.AchievementId)
        );

        return unlockedWithAchievements;
    }

    public async Task<List<AchievementDto>> GetActiveAchievementsAsync()
    {
        var activeAchievements = await _achievementRepository.GetActiveAchievementsAsync();

        if (activeAchievements == null || activeAchievements.Count == 0)
            return new List<AchievementDto>();

        return MapToDto(activeAchievements);
    }

    public async Task<List<AchievementDto>> GetAchievementsForUserAsync(Guid userId)
    {
        var unlockedAchievements = await _achievementRepository.GetAchievementsForUserAsync(userId);

        if (unlockedAchievements == null || unlockedAchievements.Count == 0)
            return new List<AchievementDto>();

        // Map UserAchievement (which contains Achievement and UnlockedAt) to the DTO
        var dtos = unlockedAchievements.Select(ua => new AchievementDto
        {
            Code = ua.Achievement?.Code ?? string.Empty,
            Name = ua.Achievement?.Name ?? string.Empty,
            Description = ua.Achievement?.Description ?? string.Empty,
            UnlockedAt = ua.UnlockedAt
        }).ToList();

        return dtos;
    }

    private static List<AchievementDto> MapToDto(List<Achievement> achievements)
    {
        var dtos = new List<AchievementDto>();
        foreach (var a in achievements)
        {
            dtos.Add(new AchievementDto
            {
                Code = a.Code,
                Name = a.Name,
                Description = a.Description
            });
        }
        return dtos;
    }

    public async Task<UserStatsDto> GetUserStatsAsync(Guid userId)
    {
        var stats = await _userStatsRepository.GetOrCreateAsync(userId);

        var dto = new UserStatsDto 
        {
            TotalGames = stats.TotalGames,
            CurrentStreakDays = stats.CurrentStreakDays,
            LongestStreakDays = stats.CurrentStreakDays
        };

        return dto;
    }
}