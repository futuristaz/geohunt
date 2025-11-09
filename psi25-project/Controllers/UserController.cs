using Microsoft.AspNetCore.Mvc;
using psi25_project.Models.Dtos;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // -------------------- Get All Users --------------------
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // -------------------- Get Current User --------------------
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrentUserAsync(User);
        if (user == null) return Unauthorized("User is not logged in.");
        return Ok(user);
    }

    // -------------------- Get Specific User --------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound("User not found.");
        return Ok(user);
    }

    // -------------------- Delete User --------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var (succeeded, errors) = await _userService.DeleteUserAsync(id);
        if (!succeeded) return BadRequest(new { Errors = errors });

        return Ok(new { Message = $"User {id} deleted successfully" });
    }
}
