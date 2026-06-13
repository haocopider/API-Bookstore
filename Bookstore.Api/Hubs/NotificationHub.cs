using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Bookstore.Api.Hubs
{
    public class NotificationHub : Hub
    {
        [Authorize]
        public override async Task OnConnectedAsync()
        {
            if (Context.User?.IsInRole("MANAGER") == true || Context.User?.IsInRole("STAFF") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            await base.OnConnectedAsync();
        }
    }
}