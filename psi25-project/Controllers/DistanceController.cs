using Microsoft.AspNetCore.Mvc;
using psi25_project.Models.Dtos;

namespace psi25_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistanceController : ControllerBase
    {

        [HttpPost]
        public IActionResult GetDistance([FromBody] DistanceDto dto)
        {
            var coords1 = dto.initialCoords;
            var coords2 = dto.guessedCoords;

            try
            {
                var distance = DistanceCalculator.CalculateHaversineDistance(coords1, coords2);
                return Ok(distance);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
