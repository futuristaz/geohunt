namespace psi25_project.Models
{
    public class Room
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string RoomCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndTime { get; set; } = null;

        public List<Player> Players { get; set; } = new();
    }
}
