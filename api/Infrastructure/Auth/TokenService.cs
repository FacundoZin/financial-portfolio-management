using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Application.Interfaces.Infrastructure.Auth;
using api.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace api.Infrastructure.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _Config;
        private readonly SymmetricSecurityKey _Key;
        public TokenService(IConfiguration config)
        {
            _Config = config;
            _Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Config["JWT:SigninKey"]));
        }

        public string CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.UserName)
            };

            var creds = new SigningCredentials(_Key, SecurityAlgorithms.HmacSha512Signature);

            var tokendescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = _Config["JWT:Issuer"],
                Audience = _Config["JWT:Audience"],
            };

            var tokenhandler = new JwtSecurityTokenHandler();

            var token = tokenhandler.CreateToken(tokendescriptor);

            return tokenhandler.WriteToken(token);
        }
    }
}