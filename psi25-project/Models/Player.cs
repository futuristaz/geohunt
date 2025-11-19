namespace psi25_project.Models
{
    public class Player
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Link to AspNetUser
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        // Session-based properties
        public Guid RoomId { get; set; }
        public Room Room { get; set; }

        public int Score { get; set; }
        public bool IsReady { get; set; }
        public string DisplayName { get; set; } = "";
    }
}