namespace psi25_project.Models
{
    public class Location
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastPlayedAt { get; set; }
        public List<Guess> Guesses { get; set; } = new();
    }
}
