using System;

namespace psi25_project.Models.Dtos
{
    public class LeaderboardEntry : IComparable<LeaderboardEntry>
    {
        public int Id { get; set; }
        public double DistanceKm { get; set; }
        public DateTime GuessedAt { get; set; }
        public int Score { get; set; }
        public int Rank { get; set; }

        // Implement IComparable
        public int CompareTo(LeaderboardEntry? other)
        {
            if (other == null) return -1;

            // Sort descending by Score, ascending by DistanceKm as tie-breaker
            int scoreComparison = other.Score.CompareTo(this.Score);
            if (scoreComparison != 0) return scoreComparison;

            return this.DistanceKm.CompareTo(other.DistanceKm);
        }
    }
}
