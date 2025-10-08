using System.ComponentModel.DataAnnotations;

namespace psi25_project.Models.Dtos
{
    public class LocationDto
    {
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public string panoId { get; set; } = string.Empty;
    }
}