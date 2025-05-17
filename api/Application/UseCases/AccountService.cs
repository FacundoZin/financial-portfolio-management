using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.Common;
using api.Application.DTOs.Account;
using api.Application.Interfaces.Infrastructure.Auth;
using api.Application.Interfaces.Infrastructure.Identity;
using api.Application.Interfaces.UseCases;
using api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace api.Application.UseCases
{
    public class AccountService : IaccountService
    {
        private readonly ITokenService _Tokenservice;
        private readonly IIdentityService _Identityservice;

        public AccountService(ITokenService tokenService, IIdentityService identityService)
        {
            _Tokenservice = tokenService;
            _Identityservice = identityService;
        }

        public async Task<AppUser?> FindByname(string name)
        {
            var user = await _Identityservice.FindByNameAsync(name);

            if (user == null) return null;

            return user;
        }

        public async Task<Result<UserDto>> LoginAsync(LoginDto loginDto)
        {
            var User = await _Identityservice.FindByNameAsync(loginDto.UserName.ToLower());

            if (User == null) return Result<UserDto>.Error("incorrect credentials", 401);

            var result = await _Identityservice.CheckPasswordAsync(User, loginDto.Password, false);

            if (!result.Succeeded) return Result<UserDto>.Error("incorrect credentials", 401);

            return Result<UserDto>.Exito(new UserDto
            {
                UserName = User.UserName!,
                Email = User.Email!,
                token = _Tokenservice.CreateToken(User)
            });
        }

        public async Task<Result<UserDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var AppUser = new AppUser
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email
                };

                var createduser = await _Identityservice.CreateAsync(AppUser, registerDto.Password!);

                if (createduser.Succeeded)
                {
                    var role = await _Identityservice.AddRoleAsync(AppUser, "User");

                    if (role.Succeeded)
                    {
                        return Result<UserDto>.Exito(new UserDto
                        {
                            UserName = AppUser.UserName!,
                            Email = AppUser.Email!,
                            token = _Tokenservice.CreateToken(AppUser)
                        });
                    }
                    else
                    {
                        return Result<UserDto>.Error(role.Errors.ToString()!,500);
                    }
                }
                else
                {
                    return Result<UserDto>.Error(createduser.Errors.ToString()!, 400);
                }
            }
            catch (Exception ex)
            {
                return Result<UserDto>.Error("sorry, something went wrong" ,500);
            }
        }
    }
}