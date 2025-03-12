namespace BlogPlatformCleanArchitecture.Domain.Entities
{
    public class PostShare
    {
        public int Id { get; set; }
        public string SharerId { get; set; } 
        public int PostId { get; set; } 
        public DateTime SharedDate { get; set; } 

        public ApplicationUser Sharer { get; set; } 
        public Post Post { get; set; } 
    }
}
