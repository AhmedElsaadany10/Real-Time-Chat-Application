using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{

    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminController(UserManager<AppUser>userManager)
        {
            _userManager = userManager;
        }
        [Authorize(Policy="AdminRole")]
        [HttpGet("user-with-roles")]
        public async Task<ActionResult>GetUsersWithRoles(){
            var users=await _userManager.Users.Include(r=>r.UserRoles).ThenInclude(r=>r.Role)
            .OrderBy(u=>u.UserName).Select(u=>new{
                u.Id,
                Username=u.UserName,
            Roles=u.UserRoles.Select(r=>r.Role.Name).ToList()
            }).ToListAsync();
            return Ok(users);
        }
        [Authorize(Policy="AdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult>EditRoles(string username ,[FromQuery]string roles){
            var selectedRoles=roles.Split(",").ToArray();
            var user =await _userManager.FindByNameAsync(username);
            if(user==null) return NotFound("user not exist");
            var userRoles=await _userManager.GetRolesAsync(user);
            var result=await _userManager.AddToRolesAsync(user,selectedRoles.Except(userRoles));
            if(!result.Succeeded) return BadRequest("Failed to add roles");
            result=await _userManager.RemoveFromRolesAsync(user,userRoles.Except(selectedRoles));
            if(!result.Succeeded) return BadRequest("Failed to remove roles");
            return Ok(await _userManager.GetRolesAsync(user));
        }
        [Authorize(Policy = "ModeratorRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}