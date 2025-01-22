using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Application.Models;
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

        public async Task<PaginatedResponseModel<Comment>> GetAllCommentsAsync(int pageNumber, int pageSize)
        {
            var totalItems = await _context.Comments.CountAsync(c => !c.IsDeleted);
            var comments = await _context.Comments
                .Where(c => !c.IsDeleted && !c.Post.IsDeleted)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.User)
                .ToListAsync();
            return new PaginatedResponseModel<Comment>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = comments
            };
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .Where(c => !c.IsDeleted && !c.Post.IsDeleted)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<PaginatedResponseModel<Comment>> GetCommentsByUserAsync(string userName, int pageNumber, int pageSize)
        {
            var totalItems = await _context.Comments.Where(c => c.User.UserName == userName).CountAsync(c => !c.IsDeleted);
            var comments = await _context.Comments
                .Where(c => c.User.UserName == userName && !c.IsDeleted && !c.Post.IsDeleted)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.User)
                .ToListAsync();
            return new PaginatedResponseModel<Comment>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = comments
            };
        }

        public async Task<PaginatedResponseModel<Comment>> GetCommentsByPostAsync(int postId, int pageNumber, int pageSize)
        {
            var totalItems = await _context.Comments.Where(c => c.PostId == postId).CountAsync(c => !c.IsDeleted);
            var comments = await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted && !c.Post.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(c => c.User)
                .ToListAsync();
            return new PaginatedResponseModel<Comment>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = comments
            };
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
