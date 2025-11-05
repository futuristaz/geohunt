using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Data;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly GeoHuntContext _context;

    public UserController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        GeoHuntContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    // -------------------- Get All Users --------------------
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
    {
        var users = await _userManager.Users
            .Select(u => new UserResponseDto
            {
                Username = u.UserName,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // -------------------- Get Single User --------------------
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound();

        return new UserResponseDto
        {
            Username = user.UserName,
            CreatedAt = user.CreatedAt
        };
    }

    // -------------------- Register New User --------------------
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] RegisterUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await _userManager.FindByNameAsync(dto.Username);
        if (existingUser != null)
            return BadRequest("Username already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Username,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new UserResponseDto
        {
            Username = user.UserName,
            CreatedAt = user.CreatedAt
        });
    }

    // -------------------- Login --------------------
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _signInManager.PasswordSignInAsync(dto.Username, dto.Password, false, false);

        if (!result.Succeeded)
            return Unauthorized("Invalid username or password.");

        return Ok("Login successful.");
    }

    // -------------------- Delete User --------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound("User not found.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok($"User {user.UserName} deleted successfully.");
    }
}
