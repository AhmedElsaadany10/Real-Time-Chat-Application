using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub:Hub
    {
        private readonly PresencState _presencState;

        public PresenceHub(PresencState presencState)
        {
            _presencState = presencState;
        }

        public override async Task OnConnectedAsync()
        {
            await _presencState.UserConnected(Context.User.GetUsername(),Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOnline",Context.User.GetUsername());

            var currentUsers=await _presencState.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers",currentUsers);

        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _presencState.UserDisconnected(Context.User.GetUsername(),Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOffline",Context.User.GetUsername());

            var currentUsers=await _presencState.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers",currentUsers);

            await base.OnDisconnectedAsync(exception);
        }
    }
}