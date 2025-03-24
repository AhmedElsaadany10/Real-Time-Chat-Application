using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Helpers;
using API.Models;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<AppUser>>GetUsersAsync();
        Task<AppUser>GetUserByIdAsync(int id);
        Task<AppUser>GetUserByUsernameAsync(string username);
        void Update(AppUser user);
        Task<bool>SaveAllAsync();
        Task<PageList<MemberDto>>GetMembersAsync(UserParams userParams);
        Task<MemberDto>GetMemberByUsernameAsync(string username);


        
    }
}