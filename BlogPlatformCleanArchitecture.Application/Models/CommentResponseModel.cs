
namespace BlogPlatformCleanArchitecture.Application.Models
{
    public class CommentResponseModel
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
