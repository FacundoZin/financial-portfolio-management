using api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace api.Application.Interfaces.Infrastructure.Identity
{
    public interface IIdentityService
    {
        Task<AppUser?> FindByNameAsync(string name);
        Task<SignInResult> CheckPasswordAsync(AppUser user, string password, bool block);
        Task<IdentityResult> CreateAsync(AppUser user, string password);
        Task<IdentityResult> AddRoleAsync(AppUser user, string role);
    }
}
