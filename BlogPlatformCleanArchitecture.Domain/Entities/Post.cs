namespace BlogPlatformCleanArchitecture.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorId { get; set; }  // Foreign key to User
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public ApplicationUser Author { get; set; }  // Navigation property to User
        public ICollection<Comment> Comments { get; set; }
        public bool IsDeleted { get; set; }
    }
}