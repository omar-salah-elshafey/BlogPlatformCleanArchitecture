using BlogPlatformCleanArchitecture.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories
{
    public interface IPostRepository {
        Task<IEnumerable<Post>> GetAllPostsAsync(); 
        Task<Post> GetPostByIdAsync(int id); 
        Task<IEnumerable<Post>> GetPostsByUserAsync(string userName); 
        Task AddPostAsync(Post post); 
        Task UpdatePostAsync(Post post); 
        Task DeletePostAsync(int id); 
    }
}
