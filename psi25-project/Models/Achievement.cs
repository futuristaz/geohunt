namespace psi25_project.Models;

public class Achievement
{
    public int Id { get; set; }

    // Unique code used internally (e.g. "BULLSEYE_100M")
    public required string Code { get; set; }

    // Display name for UI
    public required string Name { get; set; }

    // UI description
    public required string Description { get; set; }

    // Defines when the achievement should be evaluated
    public AchievementScope Scope { get; set; }

    // Allows enabling/disabling achievements without removing them
    public bool IsActive { get; set; } = true;
}

public enum AchievementScope
{
    Round = 0,  // evaluated on each guess
    Game = 1,   // evaluated after game end
    Meta = 2    // long-term achievements
}