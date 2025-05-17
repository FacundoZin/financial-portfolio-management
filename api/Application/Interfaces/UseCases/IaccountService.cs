using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Account;
using api.Domain.Entities;

namespace api.Application.Interfaces.UseCases
{
    public interface IaccountService
    {
        Task<Result<UserDto>> LoginAsync(LoginDto request);
        Task<Result<UserDto>> RegisterAsync(RegisterDto request);
        Task<AppUser?> FindByname(string name);
    }
}