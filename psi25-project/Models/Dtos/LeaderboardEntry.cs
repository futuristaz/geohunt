namespace psi25_project.Models.Dtos
{
    public class LeaderboardEntry : IComparable<LeaderboardEntry>
    {
        public int Id { get; set; }
        public double? DistanceKm { get; set; }
        public DateTime GuessedAt { get; set; }
        public int Score { get; set; }
        public int Rank { get; set; }

        public int CompareTo(LeaderboardEntry other)
        {
            // Sort descending by Score
            return other.Score.CompareTo(this.Score);
        }
    }
}
