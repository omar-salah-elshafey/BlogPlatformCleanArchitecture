using BlogPlatformCleanArchitecture.Application.DTOs;
using BlogPlatformCleanArchitecture.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogPlatformCleanArchitecture.Application.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponseModel> CreateCommentAsync(CommentDto commentDto, string userId, string userName);
        Task<IEnumerable<CommentResponseModel>> GetAllCommentsAsync();
        Task<IEnumerable<CommentResponseModel>> GetCommentsByUserAsync(string UserName);
        Task DeleteCommentAsync(int id, string userId);

        Task<CommentResponseModel> UpdateCommentAsync(int id, CommentDto commentDto, string userId, string UserName);
    }
}
