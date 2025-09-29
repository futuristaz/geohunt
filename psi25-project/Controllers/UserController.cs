using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly GeoHuntContext _context;

    public UserController(GeoHuntContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<psi25_project.Models.User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<psi25_project.Models.User>> GetUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost]
    public async Task<ActionResult<psi25_project.Models.User>> CreateUser(psi25_project.Models.User user)
    {
        // Input validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Set ID if not already set (depends on your model configuration)
        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.NewGuid();
        }

        // Initialize collections and set timestamps
        user.Games = new List<psi25_project.Models.Game>();
        user.CreatedAt = DateTime.UtcNow;

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Return 201 Created with location header pointing to the new user
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (DbUpdateException ex)
        {
            // Handle database errors (e.g., duplicate email, constraint violations)
            return BadRequest("Unable to create user. Please check your input. Error: " + ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok($"User {user.Id} deleted successfully.");
    }
}