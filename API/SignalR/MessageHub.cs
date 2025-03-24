using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using API.DTOs;
using API.Extensions;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresencState _presencState;

        public MessageHub(IMessageRepository messageRepository,IMapper mapper,IUserRepository userRepository,
        IHubContext<PresenceHub> presenceHub,PresencState presencState)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _presenceHub = presenceHub;
            _presencState = presencState;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext=Context.GetHttpContext();
            var otherUser=httpContext.Request.Query["user"].ToString();
            var groupName=GetGroupName(Context.User.GetUsername(),otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId,groupName);
            await AddToGroup(groupName);
            var message=await _messageRepository.GetMessageThread(Context.User.GetUsername(),otherUser);
            await Clients.Group(groupName).SendAsync("ReceiveMessageThread",message);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveFromMessageGroup();
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(CreateMessageDto createMessageDto){
            var username=Context.User.GetUsername();
            //if(username==createMessageDto.RecipientUsername.ToLower()) throw new HubException("You cannot chat with yourself");
            var sender=await _userRepository.GetUserByUsernameAsync(username);
            var recipient=await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            //var recipient=await _userRepository.GetUserByUsernameAsync("ahmed");
            if (recipient==null) throw new HubException("reciev not found");
            var message=new Message{
                Sender=sender,
                Recipient=recipient,
                SenderUsername=sender.UserName,
                RecipientUsername=recipient.UserName,
                Content=createMessageDto.Content
            };  
            var groupName=GetGroupName(sender.UserName,recipient.UserName);
            var group=await _messageRepository.GetGroup(groupName);
            
            if(group.Connections.Any(x=>x.Username==recipient.UserName)){
                message.DateRead=DateTime.UtcNow;
            }else{
                var connections=await _presencState.GetUserConnections(recipient.UserName);
                if(connections!=null){
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",new{username=sender.UserName,KnownAs=sender.KnownAs});
                }
            }

            
            _messageRepository.AddMessage(message); 
            if(await _messageRepository.SaveAllAsync()){
                await Clients.Group(groupName).SendAsync("NewMessage",_mapper.Map<MessageDto>(message));
            }
            
        }
        private string  GetGroupName(string sender,string recipient){
            var stringCompare=string.CompareOrdinal(sender,recipient)<0;
            return stringCompare?$"{sender}-{recipient}":$"{recipient}-{sender}";
        }
        private async Task<bool> AddToGroup(string groupName)
        {
            var group = await _messageRepository.GetGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            return await _messageRepository.SaveAllAsync();
        }

        private async Task RemoveFromMessageGroup()
        {
            var connection = await _messageRepository.GetConnection(Context.ConnectionId);
        
            _messageRepository.RemoveConnection(connection);

            await _messageRepository.SaveAllAsync();

        }
    }
}