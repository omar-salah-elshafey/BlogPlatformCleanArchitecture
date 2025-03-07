﻿using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BlogPlatformCleanArchitecture.Application.DTOs
{
    public class UpdatePostDto
    {
        [Required]
        public string Content { get; set; }
        public IFormFile? ImageFile { get; set; }
        public IFormFile? VideoFile { get; set; }
        public bool? DeleteImage { get; set; }
        public bool? DeleteVideo { get; set; }
    }
}
