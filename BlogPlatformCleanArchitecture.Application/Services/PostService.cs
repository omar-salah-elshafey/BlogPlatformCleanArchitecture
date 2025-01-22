using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostService(IPostRepository postRepository, UserManager<ApplicationUser> userManager)
        {
            _postRepository = postRepository;
            _userManager = userManager;
        }

        public async Task<PaginatedResponseModel<PostResponseModel>> GetAllPostsAsync(int pageNumber, int pageSize)
        {
            var paginatedPosts = await _postRepository.GetAllPostsAsync(pageNumber, pageSize);

            var postResponses = paginatedPosts.Items.Select(p => new PostResponseModel
            {
                Id = p.Id,
                AuthorName = p.Author.IsDeleted ? "Deleted Account" : p.Author.UserName,
                Title = p.Title,
                Content = p.Content,
                CreatedDate = p.CreatedDate,
                ModifiedDate = p.ModifiedDate,
                Comments = p.Comments
                    .Where(c => !c.IsDeleted)
                    .Select(c => new PostCommentsModel
                    {
                        CommentId = c.Id,
                        UserName = c.User.IsDeleted ? "Deleted Account" : c.User.UserName,
                        Content = c.Content,
                        CreatedDate = c.CreatedDate
                    }).OrderByDescending(c => c.CreatedDate).ToList()
            });

            return new PaginatedResponseModel<PostResponseModel>
            {
                PageNumber = paginatedPosts.PageNumber,
                PageSize = paginatedPosts.PageSize,
                TotalItems = paginatedPosts.TotalItems,
                Items = postResponses
            };
        }

        public async Task<PostResponseModel> GetPostByIdAsync(int id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
                throw new UserNotFoundException("Post Not Found!");

            return new PostResponseModel
            {
                Id = post.Id,
                AuthorName = post.Author.IsDeleted ? "Deleted Account" : post.Author.UserName,
                Title = post.Title,
                Content = post.Content,
                CreatedDate = post.CreatedDate,
                ModifiedDate = post.ModifiedDate
            };
        }

        public async Task<PaginatedResponseModel<PostResponseModel>> GetPostsByUserAsync(string userName, int pageNumber, int pageSize)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null || user.IsDeleted) throw new UserNotFoundException("User Not Found!");
            var paginatedPosts = await _postRepository.GetPostsByUserAsync(userName, pageNumber, pageSize);
            var postResponses = paginatedPosts.Items.Select(p => new PostResponseModel
            {
                Id = p.Id,
                AuthorName = p.Author.UserName,
                Title = p.Title,
                Content = p.Content,
                CreatedDate = p.CreatedDate,
                ModifiedDate = p.ModifiedDate,
                Comments = p.Comments
                .Where(c => !c.IsDeleted)
                .Select(c => new PostCommentsModel
                {
                    CommentId = c.Id,
                    UserName = c.User.UserName,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate
                }).OrderByDescending(c => c.CreatedDate).ToList()
            });
            return new PaginatedResponseModel<PostResponseModel>
            {
                PageNumber = paginatedPosts.PageNumber,
                PageSize = paginatedPosts.PageSize,
                TotalItems = paginatedPosts.TotalItems,
                Items = postResponses
            };
        }

        public async Task<PostResponseModel> CreatePostAsync(PostDto postDto, string authId, string authUserName)
        {
            if (string.IsNullOrWhiteSpace(postDto.Title) || string.IsNullOrWhiteSpace(postDto.Content))
                throw new NullOrWhiteSpaceInputException("Title or Content cannot be empty!");
            var post = new Post
            {
                AuthorId = authId,
                Title = postDto.Title.Trim(),
                Content = postDto.Content.Trim(),
                CreatedDate = DateTime.Now.ToLocalTime()
            };
            await _postRepository.AddPostAsync(post);
            return new PostResponseModel
            {
                Id = post.Id,
                AuthorName = authUserName,
                Title = post.Title,
                Content = post.Content,
                CreatedDate = post.CreatedDate,
                ModifiedDate = post.ModifiedDate,
                Comments =  new List<PostCommentsModel>()
            };
        }

        public async Task<PostResponseModel> UpdatePostAsync(int id, PostDto postDto, string userId, string authUserName)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
                throw new UserNotFoundException("Post Not Found!");
            if (post.AuthorId != userId)
                throw new ExceptionHandling.UnauthorizedAccessException("You aren't authorized to do this action!");
            if(string.IsNullOrWhiteSpace(postDto.Title) || string.IsNullOrWhiteSpace(postDto.Content))
                throw new NullOrWhiteSpaceInputException("Title or Content cannot be empty!");
            post.Title = postDto.Title.Trim();
            post.Content = postDto.Content.Trim();
            post.ModifiedDate = DateTime.Now.ToLocalTime();
            await _postRepository.UpdatePostAsync(post);
            return new PostResponseModel
            {
                Id = post.Id,
                AuthorName = authUserName,
                Title = post.Title,
                Content = post.Content,
                CreatedDate = post.CreatedDate,
                ModifiedDate = post.ModifiedDate,
                Comments = post.Comments?.Select(c => new PostCommentsModel
                {
                    CommentId = c.Id,
                    UserName = c.User.UserName,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate
                }).OrderByDescending(c => c.CreatedDate).ToList() ?? new List<PostCommentsModel>()
            };
        }

        public async Task DeletePostAsync(int id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
                throw new UserNotFoundException("Post Not Found!");
            if (!isAdmin && post.AuthorId != userId)
                throw new ExceptionHandling.UnauthorizedAccessException("You aren't authorized to do this action!");
            await _postRepository.DeletePostAsync(id);
        }
    }
}
