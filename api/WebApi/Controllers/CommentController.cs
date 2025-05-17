using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using api.Application.DTOs.Comment;
using api.Application.Helpers;
using api.Application.DTOs;
using api.Domain.Entities;
using api.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using api.Application.Interfaces.UseCases;

namespace api.WebApi.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class CommentController : ControllerBase
    {

        private readonly ICommentService _CommentService;
        private readonly IaccountService _Accountservice;

        public CommentController(ICommentService commentService, IaccountService accountservice)
        {
            _CommentService = commentService;
            _Accountservice = accountservice;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] CommentQueryObject commentQueryObject)
        {
            var comment = await _CommentService.GetAllCommentsAsync(commentQueryObject);

            return Ok(comment);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Getbyid([FromRoute] int id)
        {
            var comment = await _CommentService.GetCommentByIdAsync(id);

            if (comment.Exit == false)
            {
                return StatusCode(comment.Errorcode, comment.Errormessage);
            }

            return Ok(comment.Data);
        }

        [HttpPost("{stockid:int}")]
        public async Task<IActionResult> Create([FromRoute] string symbol, CreateCommentDto createCommentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.getUserName();
            var result = await _CommentService.CreateCommentAsync(createCommentDto, symbol, username);

            if (result.Exit == false) return StatusCode(result.Errorcode, result.Errormessage);

            return CreatedAtAction(nameof(Getbyid), new { id = result.Data.ID }, result.Data);
        }


        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var result = await _CommentService.DeleteCommentAsync(id);

            if (result.Exit == false) return StatusCode(result.Errorcode, result.Errormessage);

            return NoContent();
        }

    }
}