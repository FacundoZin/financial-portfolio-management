using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Application.DTOs.Account;
using api.Application.Interfaces.UseCases;
using api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.WebApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IaccountService _Accountservice;

        public AccountController(IaccountService accountservice)
        {
            _Accountservice = accountservice;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var User = await _Accountservice.LoginAsync(login);

            if (!User.Exit) return StatusCode(User.Errorcode, User.Errormessage);

            return Ok(User);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _Accountservice.RegisterAsync(register);

            if (!user.Exit) return StatusCode(user.Errorcode,user.Errormessage);

            return Ok(user);
        }
    }
}