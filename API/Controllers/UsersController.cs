using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository,IMapper mapper,IPhotoService photoService)
        {
            _photoService = photoService;
            _userRepository = userRepository;
            _mapper=mapper;
        }
        //[Authorize(Roles ="Admin")]
        [HttpGet]
        //[AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MemberDto>>>GetUsers([FromQuery]UserParams userParams){
            userParams.CurrentUser=User.GetUsername();
            var users=await _userRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);
        }
        //[Authorize]
        // [HttpGet("{id}")]
        // public async Task<ActionResult<MemberDto>>GetUser(int id){
        //     var user=await _userRepository.GetUserByIdAsync(id);
        //     return _mapper.Map<MemberDto>(user);
        // }
        //[Authorize(Roles ="Member")]
        [HttpGet("{username}",Name ="GetUser")]
        public async Task<ActionResult<MemberDto>>GetUser(string username){
            
            return await _userRepository.GetMemberByUsernameAsync(username);
        }
        [HttpPut]
        public async Task<ActionResult>UpdateUser(MemberUpdateDto memberUpdateDto){
            var username=User.GetUsername();
            var user=await _userRepository.GetUserByUsernameAsync(username);
            
            _mapper.Map(memberUpdateDto,user);
            _userRepository.Update(user);
            if(await _userRepository.SaveAllAsync()){
                return NoContent();
            }
            return BadRequest("failed to update user");
        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>>AddPhoto(IFormFile file){
            var username=User.GetUsername();
            var user=await _userRepository.GetUserByUsernameAsync(username);

            var result=await _photoService.AddPhotoAsync(file);
            if(result.Error!=null) return BadRequest(result.Error.Message);

            var photo=new Photo{
                Url=result.SecureUrl.AbsoluteUri,
                PublicId=result.PublicId
            };

            if(user.Photos.Count==0){
                photo.IsMain=true;
            }
            user.Photos.Add(photo);

            if(await _userRepository.SaveAllAsync()){
                //return _mapper.Map<PhotoDto>(photo);
                return CreatedAtRoute("GetUser",new{username=user.UserName},_mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Error for Adding Photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult>SetMainPhoto(int photoId){
            var username=User.GetUsername();
            var user=await _userRepository.GetUserByUsernameAsync(username);

            var photo=user.Photos.FirstOrDefault(x=>x.Id==photoId);
            if(photo.IsMain)return BadRequest("this photo is already main photo");

            var currentMain=user.Photos.FirstOrDefault(p=>p.IsMain);
            if(currentMain!=null) currentMain.IsMain=false;
            photo.IsMain=true;

            if(await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Error for Adding main Photo");
        }
    
    [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult>DeletePhoto(int photoId){
            var username=User.GetUsername();
            var user=await _userRepository.GetUserByUsernameAsync(username);

            var photo=user.Photos.FirstOrDefault(x=>x.Id==photoId);

            if(photo==null) return NotFound();
            if(photo.IsMain)return BadRequest("you cannot remove main photo");

            if(photo.PublicId!=null)
            {
                var result=await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error!=null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);
            if(await _userRepository.SaveAllAsync()) return Ok();
            return BadRequest("Something wrong for deleting photo");
        }
    }
}
