using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using System.Security.Claims;

namespace psi25_project.Services
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly GeoHuntContext context;

        public UserService(UserManager<ApplicationUser> userManager, GeoHuntContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        // -------------------- Get All Users --------------------
        public async Task<List<UserAccountDto>> GetAllUsersAsync()
        {
            var users = await userManager.Users.ToListAsync();
            var result = new List<UserAccountDto>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                result.Add(MapToDto(user, roles));
            }

            return result;
        }

        // -------------------- Get Single User by Id --------------------
        public async Task<UserAccountDto?> GetUserByIdAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;

            var roles = await userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        // -------------------- Get Current User --------------------
        public async Task<UserAccountDto?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var username = principal.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return null;

            var user = await userManager.FindByNameAsync(username);
            if (user == null) return null;

            var roles = await userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        // -------------------- Delete User --------------------
        public async Task<(bool Succeeded, IEnumerable<IdentityError>? Errors)> DeleteUserAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return (false, new[] { new IdentityError { Description = "User not found" } });

            var result = await userManager.DeleteAsync(user);
            return (result.Succeeded, result.Errors);
        }

        // -------------------- Helper Mapper --------------------
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
