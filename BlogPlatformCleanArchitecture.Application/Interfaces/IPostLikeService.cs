using BlogPlatformCleanArchitecture.Application.DTOs;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface IPostLikeService
    {
        Task<List<PostLikeDto>> ToggleLikeAsync(int postId, string userId);
        Task<List<PostLikeDto>> GetPostLikesAsync(int postId);
    }
}
