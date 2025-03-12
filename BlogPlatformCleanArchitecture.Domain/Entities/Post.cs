namespace BlogPlatformCleanArchitecture.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string AuthorId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public ApplicationUser Author { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public bool IsDeleted { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public ICollection<PostLike> Likes { get; set; }
        public ICollection<PostShare> Shares { get; set; } = new List<PostShare>();
    }
}