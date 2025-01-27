using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;

namespace BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories
{
    public interface ICommentRepository 
    {
        Task<int> GetCommentsCountAsync();
        Task<PaginatedResponseModel<Comment>> GetAllCommentsAsync(int pageNumber, int pageSize); 
        Task<Comment> GetCommentByIdAsync(int id); 
        Task<PaginatedResponseModel<Comment>> GetCommentsByUserAsync(string userName, int pageNumber, int pageSize);
        Task<PaginatedResponseModel<Comment>> GetCommentsByPostAsync(int postId, int pageNumber, int pageSize);
        Task AddCommentAsync(Comment comment); 
        Task UpdateCommentAsync(Comment comment); Task DeleteCommentAsync(int id); 
    }
}
