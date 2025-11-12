using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;
using System.Security.Claims;

namespace psi25_project.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GeoHuntContext _context;

        public UserService(UserManager<ApplicationUser> userManager, GeoHuntContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<UserAccountDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserAccountDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(MapToDto(user, roles));
            }

            return result;
        }

        public async Task<UserAccountDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        public async Task<UserAccountDto?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var username = principal.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return null;

            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        public async Task<(bool Succeeded, IEnumerable<IdentityError>? Errors)> DeleteUserAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return (false, new[] { new IdentityError { Description = "User not found" } });

            var result = await _userManager.DeleteAsync(user);
            return (result.Succeeded, result.Errors);
        }

        private static UserAccountDto MapToDto(ApplicationUser user, IEnumerable<string> roles)
        {
            return new UserAccountDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            };
        }
    }
}
