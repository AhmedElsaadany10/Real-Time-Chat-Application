using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Helpers;
using API.Models;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike>GetUserLike(int sourceUserId,int likedUersId);
        Task<AppUser>GetUserWithLikes(int userId);
        Task<PageList<LikeDto>>GetUsersLikes(LikesParams likesParams);
    }
}