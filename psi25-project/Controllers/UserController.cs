using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Models.Dtos;

namespace psi25_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //TODO - implement authorization
    //[Authorize] // require login for all user-related actions (optional)
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GeoHuntContext _context;

        public UserController(UserManager<ApplicationUser> userManager, GeoHuntContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // -------------------- Get All Users --------------------
        [HttpGet]
        //[Authorize(Roles = "Admin")] // only admins can view all users
        public async Task<ActionResult<IEnumerable<UserAccountDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var userDtos = new List<UserAccountDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserAccountDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    Roles = roles
                });
            }

            return Ok(userDtos);
        }
        // -------------------- Get Current User --------------------
        [HttpGet("me")]
        public async Task<ActionResult<UserAccountDto>> GetCurrentUser()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not logged in.");

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            return new UserAccountDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Roles = roles
            };
        }

        // -------------------- Get Specific User (Admin only) --------------------
        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserAccountDto>> GetUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            return new UserAccountDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Roles = roles
            };
        }

        // -------------------- Delete User --------------------
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
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
}
