﻿namespace BlogPlatformCleanArchitecture.Application.Models
{
    public class PostResponseModel
    {
        public int Id { get; set; }
        public string AuthorName { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public IEnumerable<PostCommentsModel> Comments { get; set; }
    }
}
