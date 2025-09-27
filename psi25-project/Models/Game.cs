namespace psi25_project.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required User User { get; set; }
        public int TotalScore { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public List<Guess> Guesses { get; set; } = new();
    }   
}
