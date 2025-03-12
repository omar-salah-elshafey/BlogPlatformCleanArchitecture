using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Infrastructure.Repositories;

namespace BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories
{
    public interface IPostRepository {
        Task<int> GetPostsCountAsync();
        Task<PaginatedResponseModel<Post>> GetAllPostsAsync(int pageNumber, int pageSize); 
        Task<Post> GetPostByIdAsync(int id); 
        Task<PaginatedResponseModel<Post>> GetPostsByUserAsync(string userName, int pageNumber, int pageSize); 
        Task AddPostAsync(Post post); 
        Task UpdatePostAsync(Post post); 
        Task DeletePostAsync(int id);
        Task AddPostShareAsync(PostShare postShare);
        Task<PostShare?> GetPostShareAsync(string userId, int postId);
        Task<PaginatedResponseModel<FeedItem>> GetUserFeedAsync(string userId, int pageNumber, int pageSize);
    }
}
