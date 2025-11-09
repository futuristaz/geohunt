using Microsoft.AspNetCore.Identity;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public class UserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserAccountDto>> GetAllUsersAsync()
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

        return userDtos;
    }

    public async Task<UserAccountDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return null;

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

    public async Task<UserAccountDto?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var username = principal.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return null;

        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return null;

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

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> DeleteUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return (false, new[] { "User not found" });

        var result = await _userManager.DeleteAsync(user);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }
}
