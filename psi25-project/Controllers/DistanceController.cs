using Microsoft.AspNetCore.Mvc;
using psi25_project.Services;

namespace psi25_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistanceController : ControllerBase
    {
        private readonly DistanceService _distanceService;

        public DistanceController(DistanceService distanceService)
        {
            _distanceService = distanceService;
        }

        [HttpGet("distance")]
        public async Task<IActionResult> GetDistance([FromQuery] string address1, [FromQuery] string address2)
        {
            try
            {
                var result = await _distanceService.CalculateDistanceAsync(address1, address2);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
