using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public CommentService(ICommentRepository commentRepository,
            UserManager<ApplicationUser> userManager,
            IPostRepository postRepository)
        {
            _commentRepository = commentRepository;
            _userManager = userManager;
            _postRepository = postRepository;
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
            var post = await _postRepository.GetPostByIdAsync(commentDto.PostId);
            if (post == null)
                return null;
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

        public async Task<bool> DeleteCommentAsync(int id, string userId, bool isAdmin)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);
            if (comment == null) return false;
            var post = await _postRepository.GetPostByIdAsync(comment.PostId);
            if (post == null) return false;
            if (!isAdmin && comment.UserId != userId && post.AuthorId != userId) return false;
            await _commentRepository.DeleteCommentAsync(id);
            return true;
        }

        public async Task<CommentResponseModel> UpdateCommentAsync(int id, CommentDto commentDto, string userId, string UserName)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);
            if (comment == null || comment.UserId != userId)
                return null;
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
