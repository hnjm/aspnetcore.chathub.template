using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using Oqtane.Shared.Enums;
using System;
using System.Linq;
using Oqtane.ChatHubs.Repository;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("blacklistuser", "[username]", new string[] { RoleNames.Everyone, RoleNames.Registered, RoleNames.Admin }, "Usage: /blacklistuser")]
    public class BlacklistUserCommand : BaseCommand
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

            if (!targetUser.Online())
            {
                await context.ChatHub.SendClientNotification("User not online.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            if (caller.UserId == targetUser.UserId)
            {
                await context.ChatHub.SendClientNotification("Calling user can not be target user.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string msg = null;
            if (args.Length > 1)
            {
                msg = String.Join(" ", args.Skip(1)).Trim();
            }

            var callerRoom = context.ChatHubRepository.GetChatHubRoom(callerContext.RoomId);
            if (caller.UserId != callerRoom.CreatorId)
            {
                await context.ChatHub.SendClientNotification("Blacklist user command can be executed from room creator only.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            if (callerRoom.Public() || callerRoom.Protected() || callerRoom.Private())
            {
                ChatHubBlacklistUser chatHubBlacklistUser = context.ChatHubRepository.AddChatHubBlacklistUser(targetUser);

                ChatHubRoomChatHubBlacklistUser room_blacklistuser = new ChatHubRoomChatHubBlacklistUser()
                {
                    ChatHubRoomId = callerRoom.Id,
                    ChatHubBlacklistUserId = chatHubBlacklistUser.Id,
                };
                context.ChatHubRepository.AddChatHubRoomChatHubBlacklistUser(room_blacklistuser);

                foreach (var connection in caller.Connections.Active())
                {
                    await context.ChatHub.SendClientNotification("Command blacklist user succeeded.", callerContext.RoomId, connection.ConnectionId, caller, ChatHubMessageType.System);
                }
            }
        }
    }
}
