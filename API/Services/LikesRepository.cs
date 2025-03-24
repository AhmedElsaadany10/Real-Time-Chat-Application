using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class LikesRepository : ILikesRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public LikesRepository(AppDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper=mapper;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUersId)
        {
            return await _context.Likes.FindAsync(sourceUserId,likedUersId);
        }

        public async Task<PageList<LikeDto>> GetUsersLikes(LikesParams likesParams)
        {
            var users=_context.Users.OrderBy(u=>u.UserName).AsQueryable();
            var likes=_context.Likes.AsQueryable();
            if(likesParams.Predicate=="liked"){
                likes=likes.Where(likes=>likes.SourceUserId==likesParams.UserId);
                users=likes.Select(likes=>likes.LikedUser);
            }
            if(likesParams.Predicate=="likedBy"){
                likes=likes.Where(likes=>likes.LikedUserId==likesParams.UserId);
                users=likes.Select(likes=>likes.SourceUser);
            }
            var query= users
            .ProjectTo<LikeDto>(_mapper.ConfigurationProvider)
            .AsNoTracking();
            return await PageList<LikeDto>.CreateAsync(query,likesParams.PageNum,likesParams.PageSize);
        }
        


        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
            .Include(x=>x.LikedUsers)
            .FirstOrDefaultAsync(x=>x.Id==userId);   
        }
    }
}