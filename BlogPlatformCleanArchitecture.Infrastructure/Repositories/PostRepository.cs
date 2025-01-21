using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogPlatformCleanArchitecture.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(int id)
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted && p.Id == id )
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByUserAsync(string userName)
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted && p.Author.UserName == userName)
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task AddPostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(int id)
        {
            var post = await _context.Posts.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);
            if (post != null)
            {
                post.IsDeleted = true;
                _context.Posts.Update(post);
                foreach (var comment in post.Comments)
                {
                    comment.IsDeleted = true;
                    _context.Comments.Update(comment);
                }
                await _context.SaveChangesAsync();
            }
        }

    }
}
