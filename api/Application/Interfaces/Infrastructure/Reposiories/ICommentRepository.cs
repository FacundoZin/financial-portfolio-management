using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Domain.Entities;
using api.Application.Helpers;

namespace api.Application.Interfaces.Infrastructure.Reposiories
{
    public interface ICommentRepository
    {
        IQueryable<Comment> GetAll();
        Task<Comment?> Getbyid(int id);
        Task<Comment?> Createasync(Comment comment);
        Task<bool> Deleteasync(int id);
    }
}