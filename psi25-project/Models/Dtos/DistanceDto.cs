namespace psi25_project.Models.Dtos
{
    public class DistanceDto
    {
        public required GeocodeResultDto initialCoords { get; set; }
        public required GeocodeResultDto guessedCoords { get; set; }
    }
}
