export interface AchievementApi {
  code: string;
  name: string;
  description: string;
  unlockedAt: string | null;
}

export interface UserAchievementApi {
  code: string;
  name: string;
  description: string;
  unlockedAt: string;
}

export interface UserStatsApi {
  totalGames: number,
  currentStreakDays: number,
  longestStreakDays: number
}

export interface AchievementDisplay extends UserAchievementApi {
  uniqueId: string;
}