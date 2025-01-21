
using System.ComponentModel.DataAnnotations;

namespace BlogPlatformCleanArchitecture.Application.DTOs
{
    public class CommentDto
    {
        [Required]
        public int PostId { get; set; }
        [Required]
        public string content { get; set; }
    }
}
