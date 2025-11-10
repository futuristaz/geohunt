using Microsoft.AspNetCore.Mvc;
using System;
using geohunt.Models.Dtos;
using geohunt.Utils;

namespace geohunt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultController : ControllerBase
    {

        [HttpPost]
        public IActionResult GetResult([FromBody] DistanceDto dto)
        {
            var coords1 = dto.initialCoords;
            var coords2 = dto.guessedCoords;

            try
            {
                var distance = DistanceCalculator.CalculateHaversineDistance(coords1, coords2, precision: 2);
                var score = ScoreCalculator.CalculateGeoScore(distance);
                return Ok(new
                {
                    dto.initialCoords,
                    dto.guessedCoords,
                    distance,
                    score,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
