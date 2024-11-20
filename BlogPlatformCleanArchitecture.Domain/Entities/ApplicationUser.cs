using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlogPlatformCleanArchitecture.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        [Required, MaxLength(50)]
        public string LastName { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow.ToLocalTime();
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; } = false;
        public List<RefreshToken>? RefreshTokens { get; set; }
        // Navigation property for the posts created by the user
        public ICollection<Post> Posts { get; set; }
        // Navigation property for the comments created by the user
        public ICollection<Comment> Comments { get; set; }
    }
}
