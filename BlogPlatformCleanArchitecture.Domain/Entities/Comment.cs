namespace BlogPlatformCleanArchitecture.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }  // Foreign key to Post
        public string UserId { get; set; }  // Foreign key to User
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow.ToLocalTime();
        public Post Post { get; set; }  // Navigation property to Post
        public ApplicationUser User { get; set; }  // Navigation property to User
        public bool IsDeleted { get; set; }
    }
}