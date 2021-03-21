using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using Oqtane.Shared.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("ciaobella", "[username]", new string[] { RoleNames.Everyone, RoleNames.Registered, RoleNames.Admin }, "Usage: /ciaobella")]
    public class CiaoBellaCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendClientNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string targetUserName = args[0];

            ChatHubUser targetUser = await context.ChatHubRepository.GetUserByDisplayName(targetUserName);
            targetUser = targetUser == null ? await context.ChatHubRepository.GetUserByUserNameAsync(targetUserName) : targetUser;
            if (targetUser == null)
            {
                await context.ChatHub.SendClientNotification("No user found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            if (caller.UserId == targetUser.UserId)
            {
                // TODO: Delete the rest of the user data
                context.ChatHubRepository.DeleteChatHubMessages(caller.UserId);
                context.ChatHubRepository.DeleteChatHubConnections(caller.UserId);
                context.ChatHubRepository.DeleteChatHubRooms(caller.UserId, context.ChatHubRepository.GetChatHubRoom(callerContext.RoomId).ModuleId);
                context.ChatHubRepository.DeleteChatHubUser(caller.UserId);

                throw new HubException(string.Format("Successfully deleted all database entries. System do not know an user named {0} anymore.", caller.DisplayName));
            }
        }
    }
}