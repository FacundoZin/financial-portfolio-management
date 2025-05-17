using api.Application.Interfaces.Infrastructure.Identity;
using api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace api.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AppUser> _UserManager;
        private readonly SignInManager<AppUser> _SigninManager;

        public IdentityService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _UserManager = userManager;
            _SigninManager = signInManager;
        }

        public async Task<AppUser?> FindByNameAsync(string name)
        {

            return await _UserManager.FindByNameAsync(name);

        }

        public async Task<SignInResult> CheckPasswordAsync(AppUser user, string password, bool block)
        {
            return await _SigninManager.CheckPasswordSignInAsync(user, password, block);
        }

        public async Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            return await _UserManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> AddRoleAsync(AppUser user, string role)
        {
            return await _UserManager.AddToRoleAsync(user, role);
        }
    }
}
