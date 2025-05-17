using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using api.Application.DTOs.Comment;
using api.Application.mappers;
using api.Application.Common;
using api.Application.Helpers;
using api.Application.Interfaces.UseCases;
using api.Application.Interfaces.Infrastructure.FMP_Client;
using api.Application.Interfaces.Infrastructure.Reposiories;

namespace api.Application.UseCases
{

    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _CommentRepo;
        private readonly IStockRepository _Stockrepo;
        private readonly IaccountService _AccountService;
        private readonly IFMPService _FMPservice;

        public CommentService(ICommentRepository commentrepo, IStockRepository stockRepository,
        IaccountService accountService, IFMPService fMPService)
        {
            _CommentRepo = commentrepo;
            _Stockrepo = stockRepository;
            _AccountService = accountService;
            _FMPservice = fMPService;
        }

        public async Task<Result<CommentDto>> CreateCommentAsync(CreateCommentDto createCommentDto, string symbol, string username)
        {
            var stockTask = _Stockrepo.GetbySymbolAsync(symbol);
            var userTask = _AccountService.FindByname(username);

            await Task.WhenAll(stockTask, userTask);

            var stock = await stockTask;
            var user = await userTask;

            if (stock == null)
            {

                var search_in_fmp = await _FMPservice.FindBySymbolAsync(symbol);

                if (search_in_fmp.Exit == false)
                {
                    return Result<CommentDto>.Error("stock not found", 404);
                }
                else
                {
                    await _Stockrepo.Createasync(search_in_fmp.Data);
                    stock = await _Stockrepo.GetbySymbolAsync(symbol);
                }
            }

            if (user == null) return Result<CommentDto>.Error("user not found", 404);

            var commentmodel = await _CommentRepo.Createasync(createCommentDto.tocommentfromCreateCommentdto(stock!.ID, user!.Id));

            return Result<CommentDto>.Exito(commentmodel!.toCommentdto());
        }

        public async Task<Result<bool>> DeleteCommentAsync(int id)
        {
            if (await _CommentRepo.Deleteasync(id) == false) return Result<bool>.Error("stock not found", 400);
            return Result<bool>.Exito(true);
        }

        public async Task<List<CommentDto>> GetAllCommentsAsync(CommentQueryObject commentQueryObject)
        {
            var consult = _CommentRepo.GetAll();

            if (!string.IsNullOrWhiteSpace(commentQueryObject.Symbol))
            {
                consult = consult.Where(s => s.Stock.Symbol == commentQueryObject.Symbol);
            }
            if (commentQueryObject.IsDecsending == true)
            {
                consult = consult.OrderByDescending(c => c.CreatedOn);
            }
            else
            {
                consult = consult.OrderBy(c => c.CreatedOn);
            }

            var comments = await consult.ToListAsync();

            return comments.Select(c => c.toCommentdto()).ToList();
        }

        public async Task<Result<CommentDto>> GetCommentByIdAsync(int id)
        {
            var comment = await _CommentRepo.Getbyid(id);

            if (comment == null)
            {
                return Result<CommentDto>.Error("comment not found", 404);
            }

            return Result<CommentDto>.Exito(comment.toCommentdto());
        }
    }
}