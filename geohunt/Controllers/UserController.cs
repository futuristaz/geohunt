
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using User = geohunt.Models.User;
using Game = geohunt.Models.Game;
using geohunt.Models.Dtos;

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
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] RegisterUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Games = new List<Game>(),
            CreatedAt = DateTime.UtcNow
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new UserResponseDto {
                Username = user.Username,
                CreatedAt = user.CreatedAt
            });
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        var passwordHasher = new PasswordHasher<User>();
        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid username or password.");
        }

        return Ok("Login successful.");
    }
}