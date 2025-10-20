using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using Location = psi25_project.Models.Location;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly GeoHuntContext _context;

    public LocationsController(GeoHuntContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
    {
        return await _context.Locations.ToListAsync();
    }

    [HttpGet("recent")]
    public async Task<ActionResult> GetRecentLocations()
    {
        var cutoffUtc = DateTime.UtcNow.AddMonths(-6);

        var result = await _context.Locations
    .AsNoTracking()
    .Where(l => l.LastPlayedAt > cutoffUtc)
    .OrderByDescending(l => l.LastPlayedAt)
    .Take(20)
    .Select(l => new
    {
        l.Id,
        l.Latitude,
        l.Longitude,
        l.panoId,
        l.LastPlayedAt,
    })
    .ToListAsync();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Location>> CreateLocation([FromBody] LocationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var location = new Location
        {
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            panoId = dto.panoId,
            CreatedAt = DateTime.UtcNow,
            LastPlayedAt = DateTime.UtcNow,
            Guesses = new List<Guess>()
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLocations), new { id = location.Id }, location);
    }

    [HttpPatch("{id}/last-played")]
    public async Task<IActionResult> UpdateLastPlayed(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
        {
            return NotFound("Location not found");
        }

        location.LastPlayedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "LastPlayedAt updated successfully",
            location.Id,
            location.LastPlayedAt
        });
    }

}
