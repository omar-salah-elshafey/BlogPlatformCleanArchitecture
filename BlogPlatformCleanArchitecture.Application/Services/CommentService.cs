using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ICommentRepository commentRepository,
            UserManager<ApplicationUser> userManager,
            IPostRepository postRepository,
            ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _userManager = userManager;
            _postRepository = postRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CommentResponseModel>> GetAllCommentsAsync()
        {
            var comments = await _commentRepository.GetAllCommentsAsync();
            return comments.Select(x => new CommentResponseModel
            {
                Id = x.Id,
                PostId = x.PostId,
                UserName = x.User.IsDeleted ? "Deleted Account" : x.User.UserName,
                Content = x.Content,
                CreatedDate = x.CreatedDate,
            });
        }

        public async Task<IEnumerable<CommentResponseModel>> GetCommentsByUserAsync(string UserName)
        {
            var user = await _userManager.FindByNameAsync(UserName);
            if (user == null || user.IsDeleted) return null;
            var comments = await _commentRepository.GetCommentsByUserAsync(UserName);
            return comments.Select(x => new CommentResponseModel
            {
                Id = x.Id,
                PostId = x.PostId,
                UserName = x.User.UserName,
                Content = x.Content,
                CreatedDate = x.CreatedDate,
            });
        }

        public async Task<CommentResponseModel> CreateCommentAsync(CommentDto commentDto, string userId, string userName)
        {
            _logger.LogWarning(userId);
            _logger.LogWarning(userName);
            _logger.LogWarning($"{commentDto.PostId}");
            var post = await _postRepository.GetPostByIdAsync(commentDto.PostId);
            if (post == null)
                throw new UserNotFoundException($"No posts were found with this ID: {commentDto.PostId}");
            var comment = new Comment
            {
                UserId = userId,
                PostId = commentDto.PostId,
                Content = commentDto.content,
                CreatedDate = DateTime.Now.ToLocalTime()
            };
            await _commentRepository.AddCommentAsync(comment);
            return new CommentResponseModel
            {
                Id = comment.Id,
                PostId = comment.PostId,
                UserName = userName,
                Content = comment.Content,
                CreatedDate = comment.CreatedDate
            };
        }

        public async Task DeleteCommentAsync(int id, string userId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);
            if (comment == null) 
                throw new UserNotFoundException($"No comments were found with this ID: {id}");
            var post = await _postRepository.GetPostByIdAsync(comment.PostId);
            if (post == null) 
                throw new UserNotFoundException($"No posts were found with this ID: {comment.PostId}");
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && comment.UserId != userId && post.AuthorId != userId)
                throw new ExceptionHandling.UnauthorizedAccessException("You aren't Authorized to do this action!");
            await _commentRepository.DeleteCommentAsync(id);
        }

        public async Task<CommentResponseModel> UpdateCommentAsync(int id, CommentDto commentDto, string userId, string UserName)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);
            if (comment == null)
                throw new UserNotFoundException($"No comments were found with this ID: {id}");
            if (comment.UserId != userId)
                throw new ExceptionHandling.UnauthorizedAccessException("You aren't Authorized to do this action!");
            comment.Content = commentDto.content;
            await _commentRepository.UpdateCommentAsync(comment);
            return new CommentResponseModel
            {
                Id = comment.Id,
                PostId = comment.PostId,
                UserName = UserName,
                Content = comment.Content,
                CreatedDate = comment.CreatedDate,
            };
        }
    }
}
