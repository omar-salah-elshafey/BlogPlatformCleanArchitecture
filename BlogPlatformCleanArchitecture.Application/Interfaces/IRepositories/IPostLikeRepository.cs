using BlogPlatformCleanArchitecture.Domain.Entities;

namespace BlogPlatformCleanArchitecture.Application.Interfaces.IRepositories
{
    public interface IPostLikeRepository
    {
        Task<bool> IsPostLikedByUserAsync(int postId, string userId);
        Task<List<PostLike>> AddPostLikeAsync(PostLike postLike);
        Task<List<PostLike>> DeletePostLikeAsync(int postId, string userId);
        Task<List<PostLike>> GetPostLikesAsync(int postId);
    }
}
