﻿using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Application.Models;
using BlogPlatformCleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PostService> _logger;
        private readonly INotificationService _notificationService;

        public PostService(IPostRepository postRepository, UserManager<ApplicationUser> userManager,
            ILogger<PostService> logger, INotificationService notificationService)
        {
            _postRepository = postRepository;
            _userManager = userManager;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<int> GetPostsCountAsync()
        {
            return await _postRepository.GetPostsCountAsync();
        }

        public async Task<PaginatedResponseModel<PostResponseModel>> GetAllPostsAsync(int pageNumber, int pageSize)
        {
            var paginatedPosts = await _postRepository.GetAllPostsAsync(pageNumber, pageSize);


            var postResponses = paginatedPosts.Items.Select(p => new PostResponseModel
            {
                Id = p.Id,
                AuthorName = p.Author.IsDeleted ? "Deleted Account" : p.Author.UserName,
                Content = p.Content,
                ImageUrl = !string.IsNullOrEmpty(p.ImageUrl) ? $"{p.ImageUrl}" : string.Empty,
                VideoUrl = !string.IsNullOrEmpty(p.VideoUrl) ? $"{p.VideoUrl}" : string.Empty,
                CreatedDate = p.CreatedDate.ToLocalTime(),
                ModifiedDate = p.ModifiedDate?.ToLocalTime(),
                Comments = p.Comments
                    .Where(c => !c.IsDeleted)
                    .Select(c => new PostCommentsModel
                    {
                        CommentId = c.Id,
                        UserName = c.User.IsDeleted ? "Deleted Account" : c.User.UserName,
                        Content = c.Content,
                        CreatedDate = c.CreatedDate.ToLocalTime()
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
                throw new NotFoundException("Post Not Found!");


            return new PostResponseModel
            {
                Id = post.Id,
                AuthorName = post.Author.IsDeleted ? "Deleted Account" : post.Author.UserName,
                Content = post.Content,
                ImageUrl = !string.IsNullOrEmpty(post.ImageUrl) ? $"{post.ImageUrl}" : string.Empty,
                VideoUrl = !string.IsNullOrEmpty(post.VideoUrl) ? $"{post.VideoUrl}" : string.Empty,
                CreatedDate = post.CreatedDate.ToLocalTime(),
                ModifiedDate = post.ModifiedDate?.ToLocalTime()
            };
        }

        public async Task<PaginatedResponseModel<PostResponseModel>> GetPostsByUserAsync(string userName, int pageNumber, int pageSize)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null || user.IsDeleted) throw new NotFoundException("User Not Found!");
            var paginatedPosts = await _postRepository.GetPostsByUserAsync(userName, pageNumber, pageSize);

            var postResponses = paginatedPosts.Items.Select(p => new PostResponseModel
            {
                Id = p.Id,
                AuthorName = p.Author.UserName,
                Content = p.Content,
                ImageUrl = !string.IsNullOrEmpty(p.ImageUrl) ? $"{p.ImageUrl}" : string.Empty,
                VideoUrl = !string.IsNullOrEmpty(p.VideoUrl) ? $"{p.VideoUrl}" : string.Empty,
                CreatedDate = p.CreatedDate.ToLocalTime(),
                ModifiedDate = p.ModifiedDate?.ToLocalTime(),
                Comments = p.Comments
                .Where(c => !c.IsDeleted)
                .Select(c => new PostCommentsModel
                {
                    CommentId = c.Id,
                    UserName = c.User.UserName,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate.ToLocalTime()
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
            if (string.IsNullOrWhiteSpace(postDto.Content))
                throw new NullOrWhiteSpaceInputException("Content cannot be empty!");
            var post = new Post
            {
                AuthorId = authId,
                Content = postDto.Content.Trim(),
                CreatedDate = DateTime.UtcNow
            };
            post.ImageUrl = await HandleFileUpload(postDto.ImageFile, "image", string.Empty);
            post.VideoUrl = await HandleFileUpload(postDto.VideoFile, "video", string.Empty);

            await _postRepository.AddPostAsync(post);

            await _notificationService.NotifyNewPostAsync(post);

            return new PostResponseModel
            {
                Id = post.Id,
                AuthorName = authUserName,
                Content = post.Content,
                ImageUrl = !string.IsNullOrEmpty(post.ImageUrl) ? $"{post.ImageUrl}" : string.Empty,
                VideoUrl = !string.IsNullOrEmpty(post.VideoUrl) ? $"{post.VideoUrl}" : string.Empty,
                CreatedDate = post.CreatedDate.ToLocalTime(),
                ModifiedDate = post.ModifiedDate?.ToLocalTime(),
                Comments =  new List<PostCommentsModel>()
            };
        }

        public async Task<PostResponseModel> UpdatePostAsync(int id, UpdatePostDto postDto, string userId, string authUserName)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
                throw new NotFoundException("Post Not Found!");
            if (post.AuthorId != userId)
                throw new ForbiddenAccessException("You aren't Authenticated to do this action!");
            if(string.IsNullOrWhiteSpace(postDto.Content))
                throw new NullOrWhiteSpaceInputException("Content cannot be empty!");
            post.Content = postDto.Content.Trim();

            post.ImageUrl = await HandleFileUpload(postDto.ImageFile, "image", post.ImageUrl);
            post.VideoUrl = await HandleFileUpload(postDto.VideoFile, "video", post.VideoUrl);

            if (postDto.DeleteImage == true)
            {
                if (!string.IsNullOrWhiteSpace(post.ImageUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImageUrl.TrimStart('/'));
                    try
                    {
                        if (File.Exists(oldImagePath))
                        {
                            File.Delete(oldImagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error deleting file: {oldImagePath}. Exception: {ex.Message}");
                        throw;
                    }
                }
                post.ImageUrl = null;
            }

            if (postDto.DeleteVideo == true)
            {
                if (!string.IsNullOrWhiteSpace(post.VideoUrl))
                {
                    var oldVideoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.VideoUrl.TrimStart('/'));
                    try
                    {
                        if (File.Exists(oldVideoPath))
                            File.Delete(oldVideoPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error deleting file: {oldVideoPath}. Exception: {ex.Message}");
                        throw;
                    }

                }
                post.VideoUrl = null;
            }

            post.ModifiedDate = DateTime.UtcNow;

            await _postRepository.UpdatePostAsync(post);
            return new PostResponseModel
            {
                Id = post.Id,
                AuthorName = authUserName,
                Content = post.Content,
                ImageUrl = !string.IsNullOrEmpty(post.ImageUrl) ? $"{post.ImageUrl}" : string.Empty,
                VideoUrl = !string.IsNullOrEmpty(post.VideoUrl) ? $"{post.VideoUrl}" : string.Empty,
                CreatedDate = post.CreatedDate.ToLocalTime(),
                ModifiedDate = post.ModifiedDate?.ToLocalTime(),
                Comments = post.Comments?.Select(c => new PostCommentsModel
                {
                    CommentId = c.Id,
                    UserName = c.User.UserName,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate.ToLocalTime()
                }).OrderByDescending(c => c.CreatedDate).ToList() ?? new List<PostCommentsModel>()
            };
        }

        public async Task DeletePostAsync(int id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin")
                || await _userManager.IsInRoleAsync(user, "SuperAdmin");
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
                throw new NotFoundException("Post Not Found!");
            if (!isAdmin && post.AuthorId != userId)
                throw new ForbiddenAccessException("You aren't Authenticated to do this action!");
            await _postRepository.DeletePostAsync(id);
        }

        public async Task<PostResponseModel> SharePostAsync(int postId, string userId, string userName)
        {
            var originalPost = await _postRepository.GetPostByIdAsync(postId);
            if (originalPost == null || originalPost.IsDeleted)
                throw new NotFoundException("Post not found or has been deleted!");

            var existingShare = await _postRepository.GetPostShareAsync(userId, postId);
            if (existingShare != null)
                throw new DuplicateValueException("You have already shared this post!");

            var postShare = new PostShare
            {
                SharerId = userId,
                PostId = postId,
                SharedDate = DateTime.UtcNow
            };

            await _postRepository.AddPostShareAsync(postShare);

            return new PostResponseModel
            {
                Id = postShare.Id,
                SharedPostId = postShare.Id,
                SharerName = userName,
                CreatedDate = postShare.SharedDate.ToLocalTime(),
                OriginalPost = new PostResponseModel
                {
                    Id = originalPost.Id,
                    AuthorName = originalPost.Author.IsDeleted ? "Deleted Account" : originalPost.Author.UserName,
                    Content = originalPost.Content,
                    ImageUrl = !string.IsNullOrEmpty(originalPost.ImageUrl) ? $"{originalPost.ImageUrl}" : string.Empty,
                    VideoUrl = !string.IsNullOrEmpty(originalPost.VideoUrl) ? $"{originalPost.VideoUrl}" : string.Empty,
                    CreatedDate = originalPost.CreatedDate.ToLocalTime(),
                    ModifiedDate = originalPost.ModifiedDate?.ToLocalTime(),
                    Comments = originalPost.Comments != null
                        ? originalPost.Comments
                            .Where(c => !c.IsDeleted)
                            .Select(c => new PostCommentsModel
                            {
                                CommentId = c.Id,
                                UserName = c.User.IsDeleted ? "Deleted Account" : c.User.UserName,
                                Content = c.Content,
                                CreatedDate = c.CreatedDate.ToLocalTime()
                            })
                            .OrderByDescending(c => c.CreatedDate)
                            .ToList()
                        : new List<PostCommentsModel>(),
                    SharedPostId = null,
                    SharerName = null,
                    OriginalPost = null
                }
            };
        }

        public async Task<PaginatedResponseModel<PostResponseModel>> GetUserFeedAsync(string userName, int pageNumber, int pageSize)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null || user.IsDeleted)
                throw new NotFoundException("User not found!");

            var paginatedFeed = await _postRepository.GetUserFeedAsync(user.Id, pageNumber, pageSize);

            var postResponses = paginatedFeed.Items.Select(feedItem =>
            {
                if (feedItem.IsPost)
                {
                    var post = (Post)feedItem.Entity;
                    return new PostResponseModel
                    {
                        Id = post.Id,
                        AuthorName = post.Author.IsDeleted ? "Deleted Account" : post.Author.UserName,
                        Content = post.Content,
                        ImageUrl = !string.IsNullOrEmpty(post.ImageUrl) ? $"{post.ImageUrl}" : string.Empty,
                        VideoUrl = !string.IsNullOrEmpty(post.VideoUrl) ? $"{post.VideoUrl}" : string.Empty,
                        CreatedDate = post.CreatedDate.ToLocalTime(),
                        ModifiedDate = post.ModifiedDate?.ToLocalTime(),
                        Comments = post.Comments
                            .Where(c => !c.IsDeleted)
                            .Select(c => new PostCommentsModel
                            {
                                CommentId = c.Id,
                                UserName = c.User.IsDeleted ? "Deleted Account" : c.User.UserName,
                                Content = c.Content,
                                CreatedDate = c.CreatedDate.ToLocalTime()
                            }).OrderByDescending(c => c.CreatedDate).ToList(),
                        SharedPostId = null,
                        SharerName = null,
                        OriginalPost = null
                    };
                }
                else
                {
                    var share = (PostShare)feedItem.Entity;
                    var originalPost = share.Post;
                    return new PostResponseModel
                    {
                        Id = share.Id,
                        SharedPostId = share.Id,
                        SharerName = share.Sharer.IsDeleted ? "Deleted Account" : share.Sharer.UserName,
                        CreatedDate = share.SharedDate.ToLocalTime(),
                        OriginalPost = new PostResponseModel
                        {
                            Id = originalPost.Id,
                            AuthorName = originalPost.Author.IsDeleted ? "Deleted Account" : originalPost.Author.UserName,
                            Content = originalPost.Content,
                            ImageUrl = !string.IsNullOrEmpty(originalPost.ImageUrl) ? $"{originalPost.ImageUrl}" : string.Empty,
                            VideoUrl = !string.IsNullOrEmpty(originalPost.VideoUrl) ? $"{originalPost.VideoUrl}" : string.Empty,
                            CreatedDate = originalPost.CreatedDate.ToLocalTime(),
                            ModifiedDate = originalPost.ModifiedDate?.ToLocalTime(),
                            Comments = originalPost.Comments
                                .Where(c => !c.IsDeleted)
                                .Select(c => new PostCommentsModel
                                {
                                    CommentId = c.Id,
                                    UserName = c.User.IsDeleted ? "Deleted Account" : c.User.UserName,
                                    Content = c.Content,
                                    CreatedDate = c.CreatedDate.ToLocalTime()
                                }).OrderByDescending(c => c.CreatedDate).ToList(),
                            SharedPostId = null,
                            SharerName = null,
                            OriginalPost = null
                        }
                    };
                }
            }).Where(response => response != null).ToList();

            return new PaginatedResponseModel<PostResponseModel>
            {
                PageNumber = paginatedFeed.PageNumber,
                PageSize = paginatedFeed.PageSize,
                TotalItems = paginatedFeed.TotalItems,
                Items = postResponses
            };
        }
        
        public async Task<PaginatedResponseModel<PostResponseModel>> GetHomeFeedAsync(int pageNumber, int pageSize)
        {
            var paginatedFeed = await _postRepository.GetHomeFeedAsync(pageNumber, pageSize);

            var postResponses = paginatedFeed.Items.Select(feedItem =>
            {
                if (feedItem.IsPost)
                {
                    var post = (Post)feedItem.Entity;
                    return new PostResponseModel
                    {
                        Id = post.Id,
                        AuthorName = post.Author.IsDeleted ? "Deleted Account" : post.Author.UserName,
                        Content = post.Content,
                        ImageUrl = !string.IsNullOrEmpty(post.ImageUrl) ? $"{post.ImageUrl}" : string.Empty,
                        VideoUrl = !string.IsNullOrEmpty(post.VideoUrl) ? $"{post.VideoUrl}" : string.Empty,
                        CreatedDate = post.CreatedDate.ToLocalTime(),
                        ModifiedDate = post.ModifiedDate?.ToLocalTime(),
                        Comments = post.Comments
                            .Where(c => !c.IsDeleted)
                            .Select(c => new PostCommentsModel
                            {
                                CommentId = c.Id,
                                UserName = c.User.IsDeleted ? "Deleted Account" : c.User.UserName,
                                Content = c.Content,
                                CreatedDate = c.CreatedDate.ToLocalTime()
                            }).OrderByDescending(c => c.CreatedDate).ToList(),
                        SharedPostId = null,
                        SharerName = null,
                        OriginalPost = null
                    };
                }
                else
                {
                    var share = (PostShare)feedItem.Entity;
                    var originalPost = share.Post;
                    return new PostResponseModel
                    {
                        Id = share.Id,
                        SharedPostId = share.Id,
                        SharerName = share.Sharer.IsDeleted ? "Deleted Account" : share.Sharer.UserName,
                        CreatedDate = share.SharedDate.ToLocalTime(),
                        OriginalPost = new PostResponseModel
                        {
                            Id = originalPost.Id,
                            AuthorName = originalPost.Author.IsDeleted ? "Deleted Account" : originalPost.Author.UserName,
                            Content = originalPost.Content,
                            ImageUrl = !string.IsNullOrEmpty(originalPost.ImageUrl) ? $"{originalPost.ImageUrl}" : string.Empty,
                            VideoUrl = !string.IsNullOrEmpty(originalPost.VideoUrl) ? $"{originalPost.VideoUrl}" : string.Empty,
                            CreatedDate = originalPost.CreatedDate.ToLocalTime(),
                            ModifiedDate = originalPost.ModifiedDate?.ToLocalTime(),
                            Comments = originalPost.Comments
                                .Where(c => !c.IsDeleted)
                                .Select(c => new PostCommentsModel
                                {
                                    CommentId = c.Id,
                                    UserName = c.User.IsDeleted ? "Deleted Account" : c.User.UserName,
                                    Content = c.Content,
                                    CreatedDate = c.CreatedDate.ToLocalTime()
                                }).OrderByDescending(c => c.CreatedDate).ToList(),
                            SharedPostId = null,
                            SharerName = null,
                            OriginalPost = null
                        }
                    };
                }
            }).Where(response => response != null).ToList();

            return new PaginatedResponseModel<PostResponseModel>
            {
                PageNumber = paginatedFeed.PageNumber,
                PageSize = paginatedFeed.PageSize,
                TotalItems = paginatedFeed.TotalItems,
                Items = postResponses
            };
        }

        public async Task DeleteSharedPostAsync(int id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin")
                || await _userManager.IsInRoleAsync(user, "SuperAdmin");
            var sharedPost = await _postRepository.GetSharedPostByIdAsync(id);
            if (sharedPost == null)
                throw new NotFoundException("Post Not Found!");
            if (!isAdmin && sharedPost.SharerId != userId)
                throw new ForbiddenAccessException("You aren't Authenticated to do this action!");
            await _postRepository.DeleteSharedPostAsync(sharedPost);
        }

        private void ValidateFileType(IFormFile file, string expectedType)
        {
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var allowedVideoExtensions = new[] { ".mp4", ".avi", ".mov" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (expectedType == "image" && !allowedImageExtensions.Contains(fileExtension))
            {
                throw new InvalidInputsException("Invalid file type. Only JPG, JPEG, PNG, or GIF files are allowed.");
            }
            if (expectedType == "video" && !allowedVideoExtensions.Contains(fileExtension))
            {
                throw new InvalidInputsException("Invalid file type. Only MP4, AVI, or MOV files are allowed.");
            }
        }

        private void ValidateMimeType(IFormFile file, string expectedType)
        {
            var allowedImageMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            var allowedVideoMimeTypes = new[] { "video/mp4", "video/avi", "video/quicktime" };

            if (expectedType == "image" && !allowedImageMimeTypes.Contains(file.ContentType))
                throw new InvalidInputsException("Invalid MIME type. Only image files are allowed.");

            if (expectedType == "video" && !allowedVideoMimeTypes.Contains(file.ContentType))
                throw new InvalidInputsException("Invalid MIME type. Only video files are allowed.");
        }

        private void ValidateExtension(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", ".mov" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidInputsException("Invalid file type. Only image and video files are allowed.");
        }

        private void ValidateFileSize(string fileExtension, long fileSize)
        {
            long maxSizeInBytes = fileExtension == ".mp4" || fileExtension == ".avi" || fileExtension == ".mov"
                ? 50 * 1024 * 1024
                : 5 * 1024 * 1024;

            if (fileSize > maxSizeInBytes)
                throw new InvalidInputsException($"File size exceeds the maximum allowed size of {maxSizeInBytes / (1024 * 1024)} MB.");
        }

        private async Task<string> HandleFileUpload(IFormFile file, string expectedType, string existingFileUrl)
        {
            if (file == null) return existingFileUrl;

            ValidateMimeType(file, expectedType);
            ValidateFileType(file, expectedType);

            if (!string.IsNullOrWhiteSpace(existingFileUrl))
            {
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingFileUrl.TrimStart('/'));
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);
            }

            return await UploadFileAsync(file);
        }

        private async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new NullOrWhiteSpaceInputException("File is empty");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", ".mov" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            ValidateExtension(file);

            ValidateFileSize(fileExtension, file.Length);


            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/posts");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/posts/{uniqueFileName}";
        }

    }
}
