namespace psi25_project.Models
{
    public class MultiplayerGame
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Room Room { get; set; } = null!;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }

        public int CurrentRound { get; set; } = 1;
        public int TotalRounds { get; set; } = 3;

        // Tracks players in this multiplayer session
        public List<MultiplayerPlayer> Players { get; set; } = new();
    }
}