using System.ComponentModel.DataAnnotations;

namespace BlogPlatformCleanArchitecture.Application.DTOs
{
    public class LoginDto
    {
        [Required, MaxLength(50)]
        public string EmailOrUserName { get; set; }
        [Required, MaxLength(50)]
        public string Password { get; set; }
    }
}
