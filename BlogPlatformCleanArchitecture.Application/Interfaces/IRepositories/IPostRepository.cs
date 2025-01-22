using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;

namespace BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories
{
    public interface IPostRepository {
        Task<PaginatedResponseModel<Post>> GetAllPostsAsync(int pageNumber, int pageSize); 
        Task<Post> GetPostByIdAsync(int id); 
        Task<PaginatedResponseModel<Post>> GetPostsByUserAsync(string userName, int pageNumber, int pageSize); 
        Task AddPostAsync(Post post); 
        Task UpdatePostAsync(Post post); 
        Task DeletePostAsync(int id); 
    }
}
