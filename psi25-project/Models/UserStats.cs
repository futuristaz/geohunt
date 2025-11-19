namespace psi25_project.Models;

public class UserStats
{
    public Guid UserId { get; set; }
    public int TotalGuesses { get; set; }
    public int TotalGames { get; set; }
    public int BestGameScore { get; set; }
    public int CurrentStreakDays { get; set; }
    public DateTime? LastPlayedDateUtc { get; set; }

    // Nav property
    public ApplicationUser? User { get; set; }
}