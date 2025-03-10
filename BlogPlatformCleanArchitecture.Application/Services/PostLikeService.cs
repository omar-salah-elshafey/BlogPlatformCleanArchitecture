using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.ExceptionHandling;
using BlogPlatformCleanArchitecture.Application.Interfaces;
using BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories;
using BlogPlatformCleanArchitecture.Domain.Entities;

namespace BlogPlatformCleanArchitecture.Application.Services
{
    public class PostLikeService : IPostLikeService
    {

        private readonly IPostLikeRepository _postLikeRepository;
        private readonly IPostRepository _postRepository;

        public PostLikeService(IPostLikeRepository postLikeRepository, IPostRepository postRepository)
        {
            _postLikeRepository = postLikeRepository;
            _postRepository = postRepository;
        }

        public async Task<List<PostLikeDto>> ToggleLikeAsync(int postId, string userId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null) throw new NotFoundException("Post not found");

            var isLiked = await _postLikeRepository.IsPostLikedByUserAsync(postId, userId);
            if (isLiked)
            {
                await _postLikeRepository.DeletePostLikeAsync(postId, userId);
                return await GetPostLikesAsync(postId);
            }
            else
            {
                var like = new PostLike
                {
                    PostId = postId,
                    UserId = userId,
                    LikedDate = DateTime.UtcNow
                };
                await _postLikeRepository.AddPostLikeAsync(like);
                return await GetPostLikesAsync(postId);
            }
        }

        public async Task<List<PostLikeDto>> GetPostLikesAsync(int postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null) throw new NotFoundException("Post not found");
            var likes = await _postLikeRepository.GetPostLikesAsync(postId);
            return likes.Select(l => new PostLikeDto
            {
                Id = l.Id,
                PostId = l.PostId,
                UserId = l.UserId,
                UserName = l.User.UserName,
                LikedDate = l.LikedDate,
                FullName = $"{l.User.FirstName} {l.User.LastName}".Trim()
            }).ToList();
        }

    }
}
