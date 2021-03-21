using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Commands
{
    public abstract class ModeratorCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext commandServicesContext, CommandCallerContext commandCallerContext, string[] args, ChatHubUser caller)
        {
            var room = commandServicesContext.ChatHubRepository.GetChatHubRoom(commandCallerContext.RoomId);
            var moderator = commandServicesContext.ChatHubRepository.GetChatHubModerator(caller.UserId);
            var chatHubRoomChatHubModerator = commandServicesContext.ChatHubRepository.GetChatHubRoomChatHubModerator(commandCallerContext.RoomId, moderator.Id);

            if(room.CreatorId != caller.UserId && chatHubRoomChatHubModerator == null && !commandServicesContext.ChatHub.Context.User.HasClaim(ClaimTypes.Role, Shared.RoleNames.Admin))
            {
                throw new HubException("You do not have any permissions to run this command.");
            }

            await ExecuteModeratorOperation(commandServicesContext, commandCallerContext, args, caller);
        }

        public abstract Task ExecuteModeratorOperation(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller);
    }
}
