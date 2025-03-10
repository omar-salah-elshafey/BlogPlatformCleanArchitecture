namespace BlogPlatformCleanArchitecture.Domain.Entities
{
    public class PostLike
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }
        public Post Post { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime LikedDate { get; set; }
    }
}
