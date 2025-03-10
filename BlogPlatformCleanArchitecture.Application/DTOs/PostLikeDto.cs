namespace BlogPlatformCleanArchitecture.Application.DTOs
{
    public record PostLikeDto
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime LikedDate { get; set; }
        public string FullName { get; set; }
    }
}
