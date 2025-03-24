using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Helpers;
using API.Models;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);

        Task<Connection>GetConnection(string connectionId);
        Task<Group>GetGroup(string groupName);

        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message>GetMessage(int id);
        Task<PageList<MessageDto>>GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>>GetMessageThread(string currentUsername,string recipientUsername);
        Task<bool>SaveAllAsync();



    }
}