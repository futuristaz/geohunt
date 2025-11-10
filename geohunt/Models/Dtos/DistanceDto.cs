using geohunt.Models;

namespace geohunt.Models.Dtos
{
    public class DistanceDto
    {
        public required Coordinate initialCoords { get; set; }
        public required Coordinate guessedCoords { get; set; }
    }
}
