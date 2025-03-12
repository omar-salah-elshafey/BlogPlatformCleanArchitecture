using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using BlogPlatformCleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatformCleanArchitecture.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetPostsCountAsync()
        {
            return await _context.Posts.CountAsync(p => !p.IsDeleted);
        }

        public async Task<PaginatedResponseModel<Post>> GetAllPostsAsync(int pageNumber, int pageSize)
        {
            var totalItems = await _context.Posts.CountAsync(p => !p.IsDeleted);
            var posts = await _context.Posts
                .Where(p => !p.IsDeleted)
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .AsSplitQuery()
                .OrderByDescending(p => p.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PaginatedResponseModel<Post>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = posts
            };
        }

        public async Task<Post> GetPostByIdAsync(int id)
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted && p.Id == id )
                .Include(p => p.Author)
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatedResponseModel<Post>> GetPostsByUserAsync(string userName, int pageNumber, int pageSize)
        {
            var totalItems = await _context.Posts.Where(p => p.Author.UserName == userName).CountAsync(p => !p.IsDeleted);
            var posts = await _context.Posts
                .Where(p => !p.IsDeleted && p.Author.UserName == userName)
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .AsSplitQuery()
                .OrderByDescending(p => p.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PaginatedResponseModel<Post>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = posts
            };
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

        public async Task AddPostShareAsync(PostShare postShare)
        {
            await _context.PostShares.AddAsync(postShare);
            await _context.SaveChangesAsync();
        }

        public async Task<PostShare?> GetPostShareAsync(string userId, int postId)
        {
            return await _context.PostShares
                .FirstOrDefaultAsync(ps => ps.SharerId == userId && ps.PostId == postId);
        }

        public async Task<PaginatedResponseModel<FeedItem>> GetUserFeedAsync(string userId, int pageNumber, int pageSize)
        {
            var postsQuery = _context.Posts
                .Where(p => p.AuthorId == userId && !p.IsDeleted)
                .Select(p => new FeedItem
                {
                    Id = p.Id,
                    SortDate = p.CreatedDate,
                    IsPost = true,
                    EntityId = p.Id,
                    SharerId = null
                });

            var sharesQuery = _context.PostShares
                .Where(ps => ps.SharerId == userId)
                .Select(ps => new FeedItem
                {
                    Id = ps.Id,
                    SortDate = ps.SharedDate,
                    IsPost = false,
                    EntityId = ps.PostId,
                    SharerId = ps.SharerId
                });

            var combinedQuery = postsQuery.Union(sharesQuery)
                .OrderByDescending(f => f.SortDate);

            var totalItems = await combinedQuery.CountAsync();
            var feedItems = await combinedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var postIds = feedItems.Where(f => f.IsPost).Select(f => f.EntityId).ToList();
            var shareIds = feedItems.Where(f => !f.IsPost).Select(f => f.Id).ToList();

            var posts = await _context.Posts
                .Where(p => postIds.Contains(p.Id) && !p.IsDeleted)
                .Include(p => p.Author)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .ToDictionaryAsync(p => p.Id);

            var shares = await _context.PostShares
                .Where(ps => shareIds.Contains(ps.Id))
                .Include(ps => ps.Sharer)
                .Include(ps => ps.Post).ThenInclude(p => p.Author)
                .Include(ps => ps.Post).ThenInclude(p => p.Comments).ThenInclude(c => c.User)
                .ToDictionaryAsync(ps => ps.Id);

            // Populate the Entity property in each FeedItem
            foreach (var feedItem in feedItems)
            {
                if (feedItem.IsPost)
                    feedItem.Entity = posts.ContainsKey(feedItem.EntityId) ? posts[feedItem.EntityId] : null;
                else
                    feedItem.Entity = shares.ContainsKey(feedItem.Id) ? shares[feedItem.Id] : null;
            }

            // Filter out items where the entity couldn't be found
            var paginatedItems = feedItems.Where(f => f.Entity != null).ToList();

            return new PaginatedResponseModel<FeedItem>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = paginatedItems
            };
        }
    }
}
