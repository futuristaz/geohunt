namespace psi25_project.Models.Dtos
{
    public class GuessResponseDto
    {
        public int Id { get; set; }
        public Guid GameId { get; set; }
        public int LocationId { get; set; }
        public int RoundNumber { get; set; }
        public double GuessedLatitude { get; set; }
        public double GuessedLongitude { get; set; }
        public double DistanceKm { get; set; }
        public int Score { get; set; }
    }
}