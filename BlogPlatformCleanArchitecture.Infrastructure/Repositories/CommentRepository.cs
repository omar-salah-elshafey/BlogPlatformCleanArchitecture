using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatformCleanArchitecture.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _context.Comments
                .Where(c => !c.IsDeleted && !c.Post.IsDeleted)
                .Include(c => c.User)
                .Include(c => c.Post)
                .ToListAsync();
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .Where(c => !c.IsDeleted && !c.Post.IsDeleted)
                .Include(c => c.User)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByUserAsync(string userName)
        {
            return await _context.Comments
                .Where(c => c.User.UserName == userName && !c.IsDeleted && !c.Post.IsDeleted)
                .Include(c => c.User)
                .Include(c => c.Post)
                .ToListAsync();
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                comment.IsDeleted = true;
                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
