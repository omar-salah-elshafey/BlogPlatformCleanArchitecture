using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentService> _logger;
        private readonly INotificationService _notificationService;

        public CommentService(ICommentRepository commentRepository,
            UserManager<ApplicationUser> userManager,
            IPostRepository postRepository,
            ILogger<CommentService> logger,
            INotificationService notificationService)
        {
            _commentRepository = commentRepository;
            _userManager = userManager;
            _postRepository = postRepository;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<int> GetCommentsCountAsync()
        {
            return await _commentRepository.GetCommentsCountAsync();
        }

        public async Task<PaginatedResponseModel<CommentResponseModel>> GetAllCommentsAsync(int pageNumber, int pageSize)
        {
            var paginatedComments = await _commentRepository.GetAllCommentsAsync(pageNumber, pageSize);

            var commentResponses = paginatedComments.Items.Select(c => new CommentResponseModel
            {
                Id = c.Id,
                PostId = c.PostId,
                UserName = c.User.IsDeleted ? "Deleted Account" : c.User.UserName,
                Content = c.Content,
                CreatedDate = c.CreatedDate,
            });
            return new PaginatedResponseModel<CommentResponseModel>
            {
                PageNumber = paginatedComments.PageNumber,
                PageSize = paginatedComments.PageSize,
                TotalItems = paginatedComments.TotalItems,
                Items = commentResponses
            };
        }

        public async Task<PaginatedResponseModel<CommentResponseModel>> GetCommentsByUserAsync(string UserName, int pageNumber, int pageSize)
        {
            var user = await _userManager.FindByNameAsync(UserName);
            if (user == null)
                throw new NotFoundException($"No user was found with this userName: {UserName}");
            var paginatedComments = await _commentRepository.GetCommentsByUserAsync(UserName, pageNumber, pageSize);
            var commentResponses = paginatedComments.Items.Select(c => new CommentResponseModel
            {
                Id = c.Id,
                PostId = c.PostId,
                UserName = c.User.UserName,
                Content = c.Content,
                CreatedDate = c.CreatedDate,
            });
            return new PaginatedResponseModel<CommentResponseModel>{
                PageNumber = paginatedComments.PageNumber,
                PageSize = paginatedComments.PageSize,
                TotalItems = paginatedComments.TotalItems,
                Items = commentResponses
            };
        }

        public async Task<PaginatedResponseModel<CommentResponseModel>> GetCommentsByPostAsync(int postId, int pageNumber, int pageSize)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
                throw new NotFoundException($"No posts were found with this ID: {postId}");
            var paginatedComments = await _commentRepository.GetCommentsByPostAsync(postId, pageNumber, pageSize);
            var commentResponses = paginatedComments.Items.Select(c => new CommentResponseModel
            {
                Id = c.Id,
                PostId = c.PostId,
                UserName = c.User.UserName,
                Content = c.Content,
                CreatedDate = c.CreatedDate,
            });
            return new PaginatedResponseModel<CommentResponseModel>{
                PageNumber = paginatedComments.PageNumber,
                PageSize = paginatedComments.PageSize,
                TotalItems = paginatedComments.TotalItems,
                Items = commentResponses
            };
        }

        public async Task<CommentResponseModel> CreateCommentAsync(CommentDto commentDto, string userId, string userName)
        {
            var post = await _postRepository.GetPostByIdAsync(commentDto.PostId);
            if (post == null)
                throw new NotFoundException($"No posts were found with this ID: {commentDto.PostId}");
            if(string.IsNullOrWhiteSpace(commentDto.content))
                throw new NullOrWhiteSpaceInputException("Content can't be null");
            var comment = new Comment
            {
                UserId = userId,
                PostId = commentDto.PostId,
                Content = commentDto.content.Trim(),
                CreatedDate = DateTime.Now.ToLocalTime()
            };
            await _commentRepository.AddCommentAsync(comment);

            if (post.AuthorId != userId)
            {
                await _notificationService.NotifyPostCommentedAsync(post, comment);
            }

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
                throw new NotFoundException($"No comments were found with this ID: {id}");
            var post = await _postRepository.GetPostByIdAsync(comment.PostId);
            if (post == null) 
                throw new NotFoundException($"No posts were found with this ID: {comment.PostId}");
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin") 
                || await _userManager.IsInRoleAsync(user, "SuperAdmin");
            if (!isAdmin && comment.UserId != userId && post.AuthorId != userId)
                throw new ForbiddenAccessException("You aren't Authenticated to do this action!");
            await _commentRepository.DeleteCommentAsync(id);
        }

        public async Task<CommentResponseModel> UpdateCommentAsync(int id, CommentDto commentDto, string userId, string UserName)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);
            if (comment == null)
                throw new NotFoundException($"No comments were found with this ID: {id}");
            if (comment.UserId != userId)
                throw new ForbiddenAccessException("You aren't Authenticated to do this action!");
            if (string.IsNullOrWhiteSpace(commentDto.content))
                throw new NullOrWhiteSpaceInputException("Content can't be null");
            comment.Content = commentDto.content.Trim();
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
