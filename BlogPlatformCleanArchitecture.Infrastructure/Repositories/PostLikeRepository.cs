using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatformCleanArchitecture.Infrastructure.Repositories
{
    public class PostLikeRepository : IPostLikeRepository
    {
        private readonly ApplicationDbContext _context;

        public PostLikeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsPostLikedByUserAsync(int postId, string userId)
        {
            return await _context.PostLikes
                .AsNoTracking()
                .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);
        }

        public async Task<List<PostLike>> AddPostLikeAsync(PostLike postLike)
        {
            await _context.PostLikes.AddAsync(postLike);
            await _context.SaveChangesAsync();
            return await GetPostLikesAsync(postLike.PostId);
        }

        public async Task<List<PostLike>> DeletePostLikeAsync(int postId, string userId)
        {
            var postLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);
            if (postLike != null)
            {
                _context.PostLikes.Remove(postLike);
                await _context.SaveChangesAsync();
            }
            return await GetPostLikesAsync(postId);
        }

        public async Task<List<PostLike>> GetPostLikesAsync(int postId)
        {
            return await _context.PostLikes
                .AsNoTracking()
                .Where(pl => pl.PostId == postId)
                .Include(pl => pl.User)
                .OrderByDescending(pl => pl.LikedDate)
                .ToListAsync();
        }
    }
}
