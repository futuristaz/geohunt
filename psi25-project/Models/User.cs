using System.Collections;

namespace psi25_project.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<Game> Games { get; set; } = new List<Game>();
    }
}
