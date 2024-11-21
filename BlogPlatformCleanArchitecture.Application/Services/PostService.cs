using BlogPlatformCleanArchitecture.Application.DTOs;
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

        public async Task<IEnumerable<PostResponseModel>> GetAllPostsAsync()
        {
            var posts = await _postRepository.GetAllPostsAsync();
            return posts.Select(p => new PostResponseModel
            {
                Id = p.Id,
                AuthorName = p.Author.IsDeleted ? "Deleted Account" : p.Author.UserName,
                Title = p.Title,
                Content = p.Content,
                CreatedDate = p.CreatedDate,
                ModifiedDate = p.ModifiedDate,
                Comments = p.Comments.Select(c => new PostCommentsModel
                {
                    UserName = c.User.IsDeleted ? "Deleted Account" : c.User.UserName,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate
                }).ToList()
            });
        }

        public async Task<PostResponseModel> GetPostByIdAsync(int id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
                return null;
            return new PostResponseModel
            {
                Id = post.Id,
                AuthorName = post.Author.IsDeleted ? "Deleted Account" : post.Author.UserName,
                Title = post.Title,
                Content = post.Content,
                CreatedDate = post.CreatedDate,
                ModifiedDate = post.ModifiedDate,
                Comments = post.Comments.Select(c => new PostCommentsModel
                {
                    UserName = c.User.IsDeleted ? "Deleted Account" : c.User.UserName,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate
                }).ToList()
            };
        }

        public async Task<IEnumerable<PostResponseModel>> GetPostsByUserAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null || user.IsDeleted) return null;
            var posts = await _postRepository.GetPostsByUserAsync(userName);
            return posts.Select(p => new PostResponseModel
            {
                Id = p.Id,
                AuthorName = p.Author.UserName,
                Title = p.Title,
                Content = p.Content,
                CreatedDate = p.CreatedDate,
                ModifiedDate = p.ModifiedDate,
                Comments = p.Comments.Select(c => new PostCommentsModel
                {
                    UserName = c.User.UserName,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate
                }).ToList()
            });
        }

        public async Task<PostResponseModel> CreatePostAsync(PostDto postDto, string authId, string authUserName)
        {
            var post = new Post
            {
                AuthorId = authId,
                Title = postDto.Title,
                Content = postDto.Content,
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
                Comments = post.Comments?.Select(c => new PostCommentsModel
                {
                    UserName = c.User.UserName,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate
                }).ToList() ?? new List<PostCommentsModel>() // Ensure Comments are not null
            };
        }


        public async Task<PostResponseModel> UpdatePostAsync(int id, PostDto postDto, string authId, string authUserName)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null || post.AuthorId != authId)
                return null;
            post.Title = postDto.Title;
            post.Content = postDto.Content;
            post.ModifiedDate = DateTime.Now.ToLocalTime();
            await _postRepository.UpdatePostAsync(post);
            return new PostResponseModel
            {
                Id = post.Id,
                AuthorName = authUserName,
                Title = post.Title,
                Content = post.Content,
                CreatedDate = post.CreatedDate,
                ModifiedDate = post.ModifiedDate
            };
        }

        public async Task<bool> DeletePostAsync(int id, string userId, bool isAdmin)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null || (!isAdmin && post.AuthorId != userId))
                return false;
            await _postRepository.DeletePostAsync(id);
            return true;
        }
    }
}
