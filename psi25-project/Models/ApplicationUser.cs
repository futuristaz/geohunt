using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace psi25_project.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        // These two come from your old User table
        public DateTime CreatedAt { get; set; }

        // Preserve relationships
        public List<Game> Games { get; set; } = new List<Game>();
    }
}
