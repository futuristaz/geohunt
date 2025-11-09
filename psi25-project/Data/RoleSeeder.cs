using Microsoft.AspNetCore.Identity;

public static class RoleSeeder
{
    public static async Task SeedRoles(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles = new[] { "Admin", "Player" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }
}

