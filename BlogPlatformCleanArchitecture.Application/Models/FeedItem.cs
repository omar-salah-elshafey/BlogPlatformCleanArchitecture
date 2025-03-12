namespace BlogPlatformCleanArchitecture.Infrastructure.Repositories
{
    public class FeedItem
    {
        public int Id { get; set; } // Post.Id or PostShare.Id
        public DateTime SortDate { get; set; } // CreatedDate or SharedDate
        public bool IsPost { get; set; } // To distinguish type
        public int EntityId { get; set; } // ID of the Post (for both Post and PostShare)
        public string? SharerId { get; set; } // Sharer ID for PostShare
        public object Entity { get; set; } // Holds Post or PostShare (populated after query)
    }
}
