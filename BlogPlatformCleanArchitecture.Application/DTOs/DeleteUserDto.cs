using System.ComponentModel.DataAnnotations;

namespace BlogPlatformCleanArchitecture.Application.DTOs
{
    public class DeleteUserDto
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        public string? RefreshToken { get; set; }
        public string? Password { get; set; }
    }
}
