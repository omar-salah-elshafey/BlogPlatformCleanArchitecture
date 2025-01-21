using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Models;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostResponseModel>> GetAllPostsAsync();
        Task<PostResponseModel> GetPostByIdAsync(int id);
        Task<IEnumerable<PostResponseModel>> GetPostsByUserAsync(string UserName);
        Task<PostResponseModel> CreatePostAsync(PostDto postDto, string authId, string authUserName);
        Task<PostResponseModel> UpdatePostAsync(int id, PostDto postDto, string userId, string authUserName);
        Task DeletePostAsync(int id, string userId);
    }
}
