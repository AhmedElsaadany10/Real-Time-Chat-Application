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
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;
                private readonly IMapper _mapper;
        public MessageRepository(AppDbContext context,IMapper mapper)
        {
            _context=context;
            _mapper=mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroup(string groupName)
        {
                return await _context.Groups.Include(x=>x.Connections).FirstOrDefaultAsync(n=>n.Name==groupName);

        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
            .Include(x=>x.Sender).Include(x=>x.Recipient)
            .SingleOrDefaultAsync(m=>m.Id==id);
        }

        public async Task<PageList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query=_context.Messages
            .OrderByDescending(m=>m.MessageSent)
            .AsQueryable();
            query=messageParams.Container switch{
                "Inbox"=>query.Where(u=>u.Recipient.UserName==messageParams.Username&&u.RecipientDelete==false),
                "Outbox"=>query.Where(u=>u.Sender.UserName==messageParams.Username&&u.SenderDelete==false),
                _ =>query.Where(u=>u.Recipient.UserName==messageParams.Username&&u.RecipientDelete==false&&u.DateRead==null)
            };
            var messages=query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            return await PageList<MessageDto>.CreateAsync(messages,messageParams.PageNum,messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages=await _context.Messages
            .Include(x=>x.Sender).ThenInclude(p=>p.Photos)
            .Include(x=>x.Recipient).ThenInclude(p=>p.Photos)
            .Where(m=>m.Recipient.UserName==currentUsername
                    &&m.RecipientDelete==false
                    && m.Sender.UserName==recipientUsername
                    ||m.Recipient.UserName==recipientUsername
                    &&m.Sender.UserName==currentUsername
                    &&m.SenderDelete==false)
            .OrderBy(m=>m.MessageSent)        
            .ToListAsync();
            var unreadMessages=messages.Where(m=>m.DateRead==null&&m.Recipient.UserName==currentUsername).ToList();
            if(unreadMessages.Any()){
                foreach(var message in unreadMessages){
                    message.DateRead=DateTime.Now;
                }
                await _context.SaveChangesAsync();
            }
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync()>0;
        }
    }
}