using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Models;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IPostService
    {
        Task<int> GetPostsCountAsync();
        Task<PaginatedResponseModel<PostResponseModel>> GetAllPostsAsync(int pageNumber, int pageSize);
        Task<PostResponseModel> GetPostByIdAsync(int id);
        Task<PaginatedResponseModel<PostResponseModel>> GetPostsByUserAsync(string UserName, int pageNumber, int pageSize);
        Task<PostResponseModel> CreatePostAsync(PostDto postDto, string authId, string authUserName);
        Task<PostResponseModel> UpdatePostAsync(int id, UpdatePostDto postDto, string userId, string authUserName);
        Task DeletePostAsync(int id, string userId);
        Task<PostResponseModel> SharePostAsync(int postId, string userId, string userName);
        Task<PaginatedResponseModel<PostResponseModel>> GetUserFeedAsync(string userName, int pageNumber, int pageSize);
        Task<PaginatedResponseModel<PostResponseModel>> GetHomeFeedAsync(int pageNumber, int pageSize);
    }
}
