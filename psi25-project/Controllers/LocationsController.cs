using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<IEnumerable<psi25_project.Models.Location>>> GetLocations()
    {
        return await _context.Locations.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<psi25_project.Models.Location>> CreateLocation(psi25_project.Models.Location location)
    {
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLocations), new { id = location.Id }, location);
    }
    

}