using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BlogPlatformCleanArchitecture.Application.DTOs
{
    public class PostDto
    {
        [Required]
        public string Content { get; set; }
        public IFormFile? ImageFile { get; set; }
        public IFormFile? VideoFile { get; set; }
    }
}
