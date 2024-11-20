using System.ComponentModel.DataAnnotations;

namespace BlogPlatformCleanArchitecture.Application.DTOs
{
    public class ConfirmEmailDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
