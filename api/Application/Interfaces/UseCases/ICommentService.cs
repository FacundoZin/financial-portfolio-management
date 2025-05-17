using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Comment;
using api.Application.Helpers;
using api.Domain.Entities;

namespace api.Application.Interfaces.UseCases
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetAllCommentsAsync(CommentQueryObject commentQueryObject);
        Task<Result<CommentDto>> GetCommentByIdAsync(int id);
        Task<Result<CommentDto>> CreateCommentAsync(CreateCommentDto createCommentDto, string symbol, string username);
        Task<Result<bool>> DeleteCommentAsync(int id);

    }
}