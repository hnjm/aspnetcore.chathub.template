using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("clear", "[]", new string[] { RoleNames.Everyone, RoleNames.Registered, RoleNames.Admin } , "Usage: /clear | /recycle")]
    public class ClearCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            await context.ChatHub.Clients.Client(callerContext.ConnectionId).SendAsync("ClearHistory", callerContext.RoomId);

        }
    }
}