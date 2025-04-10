using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Threading.Tasks;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
                private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository,IMessageRepository messageRepository,IMapper mapper)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _mapper=mapper;
        }
        [HttpPost]
        public async Task<ActionResult<MessageDto>>CreateMessage(CreateMessageDto createMessageDto){
            var username=User.GetUsername();
            //if(username==createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot chat with yourself");
            var sender=await _userRepository.GetUserByUsernameAsync(username);
            var recipient=await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            //var recipient=await _userRepository.GetUserByUsernameAsync("ahmed");
            if (recipient==null)return BadRequest("reciev not found");
            var message=new Message{
                Sender=sender,
                Recipient=recipient,
                SenderUsername=sender.UserName,
                RecipientUsername=recipient.UserName,
                Content=createMessageDto.Content
            };  
            
            _messageRepository.AddMessage(message); 
            if(await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));
            return BadRequest("failed to send message");
        }
        [HttpGet]
        public async Task<IEnumerable<MessageDto>>GetMessagesForUser([FromQuery]MessageParams messageParams){
            messageParams.Username=User.GetUsername();
            var messages=await _messageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,messages.TotalCount,messages.TotalPages);
            return messages;
        }
        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>>GetMessageThread(string username){
            var currentUsername=User.GetUsername();
            return Ok(await _messageRepository.GetMessageThread(currentUsername,username));
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult>DeleteMessage(int id){
            var username=User.GetUsername();
            var message=await _messageRepository.GetMessage(id);
            if(message.Sender.UserName!=username&&message.Recipient.UserName!=username)
            return Unauthorized();
            if(username==message.Sender.UserName)
                message.SenderDelete=true;
            if(username==message.Recipient.UserName)
            message.RecipientDelete=true;
            if(message.SenderDelete&&message.RecipientDelete)
            _messageRepository.DeleteMessage(message);
            if(await _messageRepository.SaveAllAsync()) return Ok();
            return BadRequest("Something wrong to delete this message");
        }
    }
}