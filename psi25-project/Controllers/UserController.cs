
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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


    public class RegisterUserDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<ActionResult<psi25_project.Models.User>> CreateUser([FromBody] RegisterUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new psi25_project.Models.User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Games = new List<psi25_project.Models.Game>(),
            CreatedAt = DateTime.UtcNow
        };

        var passwordHasher = new PasswordHasher<psi25_project.Models.User>();
        user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (DbUpdateException ex)
        {
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