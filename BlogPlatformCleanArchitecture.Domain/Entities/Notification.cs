namespace BlogPlatformCleanArchitecture.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Who receives the notification
        public string Message { get; set; }
        public string Type { get; set; } // e.g., "NewPost", "PostLiked", "PostCommented"
        public int? RelatedPostId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; }
        public Post RelatedPost { get; set; }
    }
}
