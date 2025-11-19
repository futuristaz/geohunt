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
        AchievementCodes.CleanSweep
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
        stats.LastPlayedDateUtc = DateTime.UtcNow;

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
                Id = Guid.NewGuid(),
                UserId = userId,
                AchievementId = a.Id,
                UnlockedAt = DateTime.UtcNow
            })
            .ToList();

        if (newUnlocks.Count > 0)
            await _achievementRepository.AddNewlyUnlockedAchievementsAsync(newUnlocks);

        return newUnlocks;
    }

        public async Task<IReadOnlyList<UserAchievement>> OnGameFinishedAsync(Guid userId, Guid gameId, int totalScore, int totalRounds)
        {
            // 1) Load relevant achievements
            var catalog = await _achievementRepository.GetActiveByCodesAsync(GameFinishedCodes);

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
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AchievementId = a.Id,
                    UnlockedAt = DateTime.UtcNow
                })
                .ToList();

            if (newUnlocks.Count > 0)
                await _achievementRepository.AddNewlyUnlockedAchievementsAsync(newUnlocks);

            return newUnlocks;
        }
}