using BlogPlatformCleanArchitecture.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories
{
    public interface ICommentRepository 
    { 
        Task<IEnumerable<Comment>> GetAllCommentsAsync(); 
        Task<Comment> GetCommentByIdAsync(int id); 
        Task<IEnumerable<Comment>> GetCommentsByUserAsync(string userName); 
        Task AddCommentAsync(Comment comment); 
        Task UpdateCommentAsync(Comment comment); Task DeleteCommentAsync(int id); 
    }
}
