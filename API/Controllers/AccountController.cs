using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Interfaces;
using API.Models;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser>signInManager,ITokenService tokenService,IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService=tokenService;
            _mapper=mapper;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){
            if(await UserExists(registerDto.Username)) return BadRequest("Username is already Exist");
            var user=_mapper.Map<AppUser>(registerDto);
            
                user.UserName=registerDto.Username.ToLower();
                

            var result=await _userManager.CreateAsync(user,registerDto.Password);
            if(!result.Succeeded) return BadRequest(result.Errors);
            var roleResult= await _userManager.AddToRoleAsync(user,Roles.Member.ToString());
            if(!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            return new UserDto{
                Username=user.UserName,
                Token=await _tokenService.CreateToken(user)
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            var user=await _userManager.Users.Include(p=>p.Photos).SingleOrDefaultAsync(u=>u.UserName==loginDto.Username.ToLower());
            if(user==null) return Unauthorized("Invalid Username");

            var result=await _signInManager.CheckPasswordSignInAsync(user,loginDto.Password,false);
            if(!result.Succeeded) return Unauthorized();
            return new UserDto{
                KnownAs=user.KnownAs,
                Username=user.UserName,
                Token=await _tokenService.CreateToken(user),
                PhotoUrl=user.Photos.FirstOrDefault(p=>p.IsMain)?.Url,
            };
        }
        private async Task<bool> UserExists(string username){
            return await _userManager.Users.AnyAsync(u=>u.UserName==username.ToLower());
        }
    }
}