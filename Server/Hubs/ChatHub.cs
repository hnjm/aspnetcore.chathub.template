using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using System.Text.RegularExpressions;
using Oqtane.Shared.Enums;
using System.Linq;
using Oqtane.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Oqtane.Shared;
using Microsoft.EntityFrameworkCore;
using Oqtane.Shared.Models;
using Oqtane.ChatHubs.Services;
using Oqtane.ChatHubs.Repository;
using Oqtane.ChatHubs.Commands;
using Oqtane.Modules;
using System.Security.Claims;

namespace Oqtane.ChatHubs.Hubs
{

    [AllowAnonymous]
    public class ChatHub : Hub, IService
    {

        private readonly IUserRepository userRepository;
        private readonly IChatHubRepository chatHubRepository;
        private readonly IChatHubService chatHubService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IRoleRepository roles;
        private readonly IUserRoleRepository userRoles;

        public static Dictionary<int, string> VideoFirstChunks = new Dictionary<int, string>();

        public ChatHub(
            IUserRepository userRepository,
            IChatHubRepository chatHubRepository,
            IChatHubService chatHubService,
            UserManager<IdentityUser> identityUserManager,
            IRoleRepository roles,
            IUserRoleRepository userRoles
            )
        {
            this.userRepository = userRepository;
            this.chatHubRepository = chatHubRepository;
            this.chatHubService = chatHubService;
            this.userManager = identityUserManager;
            this.roles = roles;
            this.userRoles = userRoles;
        }

        private async Task<ChatHubUser> GetChatHubUserAsync()
        {
            ChatHubUser user = await this.IdentifyUser();
            if (user == null)
            {
                user = await this.IdentifyGuest(Context.ConnectionId);
            }

            if (user == null)
            {
                throw new HubException("No valid user found.");
            }

            return user;
        }
        public async Task<ChatHubUser> IdentifyGuest(string connectionId)
        {
            ChatHubConnection connection = await Task.Run(() => chatHubRepository.GetConnectionByConnectionId(connectionId));
            if (connection != null)
            {
                return await this.chatHubRepository.GetUserByIdAsync(connection.User.UserId);
            }

            return null;
        }
        public async Task<ChatHubUser> IdentifyUser()
        {
            var username = Context.User.Identity.Name;

            if (Context.User.Identity.IsAuthenticated)
            {
                ChatHubUser chatHubUser = await this.chatHubRepository.GetUserByUserNameAsync(username);
                if (chatHubUser == null)
                {
                    User user = this.userRepository.GetUser(username);
                    if (user != null)
                    {
                        await this.chatHubRepository.UpdateUserAsync(user);
                        chatHubUser = await this.chatHubRepository.GetUserByUserNameAsync(username);
                    }
                }

                return chatHubUser;
            }

            return null;
        }

        private async Task<ChatHubUser> OnConnectedGuest()
        {
            string guestname = null;
            guestname = Context.GetHttpContext().Request.Query["guestname"];
            guestname = guestname.Trim();

            if (string.IsNullOrEmpty(guestname) || !this.IsValidGuestUsername(guestname))
            {
                throw new HubException("No valid username.");
            }

            string username = this.CreateUsername(guestname);
            string displayname = this.CreateDisplaynameFromUsername(username);

            if (await this.chatHubRepository.GetUserByDisplayName(displayname) != null)
            {
                throw new HubException("Displayname already in use. Goodbye.");
            }

            string email = "noreply@anyways.tv";
            string password = "§PasswordPolicy42";

            ChatHubUser chatHubUser = new ChatHubUser()
            {
                SiteId = 1,
                Username = username,
                DisplayName = displayname,
                Email = email,
                LastIPAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
            };
            chatHubUser = this.chatHubRepository.AddChatHubUser(chatHubUser);

            if (chatHubUser != null && chatHubUser.Username != RoleNames.Host)
            {
                List<Role> roles = this.roles.GetRoles(chatHubUser.SiteId).Where(item => item.IsAutoAssigned).ToList();
                foreach (Role role in roles)
                {
                    UserRole userrole = new UserRole();
                    userrole.UserId = chatHubUser.UserId;
                    userrole.RoleId = role.RoleId;
                    userrole.EffectiveDate = null;
                    userrole.ExpiryDate = null;
                    userRoles.AddUserRole(userrole);
                }
            }

            ChatHubConnection ChatHubConnection = new ChatHubConnection()
            {
                ChatHubUserId = chatHubUser.UserId,
                ConnectionId = Context.ConnectionId,
                IpAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
                UserAgent = Context.GetHttpContext().Request.Headers["User-Agent"].ToString(),
                Status = Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)
            };
            ChatHubConnection = this.chatHubRepository.AddChatHubConnection(ChatHubConnection);

            ChatHubSettings ChatHubSetting = new ChatHubSettings()
            {
                UsernameColor = "#7744aa",
                MessageColor = "#44aa77",
                ChatHubUserId = chatHubUser.UserId
            };
            ChatHubSetting = this.chatHubRepository.AddChatHubSetting(ChatHubSetting);

            return chatHubUser;
        }
        private async Task<ChatHubUser> OnConnectedUser(ChatHubUser chatHubUser)
        {
            ChatHubConnection ChatHubConnection = new ChatHubConnection()
            {
                ChatHubUserId = chatHubUser.UserId,
                ConnectionId = Context.ConnectionId,
                IpAddress = Context.GetHttpContext().Connection.RemoteIpAddress.ToString(),
                UserAgent = Context.GetHttpContext().Request.Headers["User-Agent"].ToString(),
                Status = Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)
            };
            ChatHubConnection = this.chatHubRepository.AddChatHubConnection(ChatHubConnection);

            ChatHubSettings ChatHubSetting = this.chatHubRepository.GetChatHubSettingByUser(chatHubUser);
            if(ChatHubSetting == null)
            {
                ChatHubSetting = new ChatHubSettings()
                {
                    UsernameColor = "#7744aa",
                    MessageColor = "#44aa77",
                    ChatHubUserId = chatHubUser.UserId
                };
                ChatHubSetting = this.chatHubRepository.AddChatHubSetting(ChatHubSetting);
            }

            return chatHubUser;
        }
        [AllowAnonymous]
        public override async Task OnConnectedAsync()
        {
            string platform = Context.GetHttpContext().Request.Headers["platform"];
            string moduleId = Context.GetHttpContext().Request.Headers["moduleid"];
            List<ChatHubRoom> list = this.chatHubRepository.GetChatHubRoomsByModuleId(int.Parse(moduleId)).ToList();

            ChatHubUser user = await this.IdentifyUser();
            if (user != null)
            {
                user = await this.OnConnectedUser(user);
            }
            else
            {
                user = await this.OnConnectedGuest();
            }

            ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
            await Clients.Clients(user.Connections.Select(item => item.ConnectionId).ToArray<string>()).SendAsync("OnUpdateConnectedUser", chatHubUserClientModel);
            await base.OnConnectedAsync();
        }

        [AllowAnonymous]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            var rooms = chatHubRepository.GetChatHubRoomsByUser(user).Enabled();
            foreach (var room in await rooms.ToListAsync())
            {
                if (user.Connections.Active().Count() == 1)
                {
                    var clientModel = this.chatHubService.CreateChatHubUserClientModel(user);
                    await Clients.Group(room.Id.ToString()).SendAsync("RemoveUser", clientModel, room.Id.ToString());
                }

                await this.SendGroupNotification(string.Format("{0} disconnected from chat with client device {1}.", user.DisplayName, this.chatHubService.MakeStringAnonymous(Context.ConnectionId, 7, '*')), room.Id, Context.ConnectionId, user, ChatHubMessageType.Connect_Disconnect);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Id.ToString());
            }

            if (!Context.User.HasClaim(ClaimTypes.Role, RoleNames.Registered) && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin) && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Host))
            {
                var roomsByCreator = this.chatHubRepository.GetChatHubRoomsByCreator(user.UserId);
                foreach (var room in await roomsByCreator.ToListAsync())
                {
                    room.Status = ChatHubRoomStatus.Archived.ToString();
                    this.chatHubRepository.UpdateChatHubRoom(room);
                }
            }

            var connection = await this.chatHubRepository.GetConnectionByConnectionId(Context.ConnectionId);
            connection.Status = Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Inactive);
            chatHubRepository.UpdateChatHubConnection(connection);

            ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
            await Clients.Clients(user.Connections.Select(item => item.ConnectionId)).SendAsync("OnUpdateConnectedUser", chatHubUserClientModel);

            await base.OnDisconnectedAsync(exception);
        }

        [AllowAnonymous]
        public async Task Init()
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            var rooms = this.chatHubRepository.GetChatHubRoomsByUser(user).Public().Enabled().ToList();
            rooms.AddRange(this.chatHubRepository.GetChatHubRoomsByUser(user).Private().Enabled().ToList());

            if (Context.User.Identity.IsAuthenticated)
            {
                rooms.AddRange(this.chatHubRepository.GetChatHubRoomsByUser(user).Protected().Enabled().ToList());
            }
            
            foreach (var room in rooms)
            {
                await this.EnterChatRoom(room.Id);
            }
        }

        [AllowAnonymous]
        public async Task EnterChatRoom(int roomId)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();
            ChatHubRoom room = chatHubRepository.GetChatHubRoom(roomId);

            if(room.Status == ChatHubRoomStatus.Archived.ToString())
            {
                if (!Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin) && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Host))
                {
                    throw new HubException("You cannot enter an archived room.");
                }
            }

            if(room.Public() || room.Protected())
            {
                if (this.chatHubService.IsBlacklisted(room, user))
                {
                    throw new HubException("You have been added to blacklist for this room.");
                }
            }

            if(room.Protected())
            {
                if (!Context.User.Identity.IsAuthenticated)
                {
                    throw new HubException("This room is for authenticated user only.");
                }
            }

            if (room.Private())
            {
                if (!this.chatHubService.IsWhitelisted(room, user))
                {
                    if(room.CreatorId != user.UserId)
                    {
                        await this.AddWaitingRoomItem(user, room);
                        throw new HubException("No valid private room connection. You have been added to waiting list.");
                    }
                }
            }

            if (room.OneVsOne())
            {
                if (!this.chatHubService.IsValidOneVsOneConnection(room, user))
                {
                    throw new HubException("No valid one vs one room id.");
                }
            }

            if (room.Public() || room.Protected() || room.Private() || room.OneVsOne())
            {
                ChatHubRoomChatHubUser room_user = new ChatHubRoomChatHubUser()
                {
                    ChatHubRoomId = room.Id,
                    ChatHubUserId = user.UserId
                };
                chatHubRepository.AddChatHubRoomChatHubUser(room_user);

                ChatHubRoom chatHubRoomClientModel = await this.chatHubService.CreateChatHubRoomClientModelAsync(room);

                foreach (var connection in user.Connections.Active())
                {
                    await Groups.AddToGroupAsync(connection.ConnectionId, room.Id.ToString());
                    await Clients.Client(connection.ConnectionId).SendAsync("AddRoom", chatHubRoomClientModel);
                }

                ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
                await Clients.Group(room.Id.ToString()).SendAsync("AddUser", chatHubUserClientModel, room.Id.ToString());

                await this.SendGroupNotification(string.Format("{0} entered chat room with client device {1}.", user.DisplayName, this.chatHubService.MakeStringAnonymous(Context.ConnectionId, 7, '*')), room.Id, Context.ConnectionId, user, ChatHubMessageType.Enter_Leave);
            }
        }
        [AllowAnonymous]
        public async Task LeaveChatRoom(int roomId)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();
            ChatHubRoom room = chatHubRepository.GetChatHubRoom(roomId);

            if (!this.chatHubRepository.GetChatHubUsersByRoom(room).Any(item => item.UserId == user.UserId))
            {
                throw new HubException("User already left room.");
            }

            if (room.Public() || room.Protected())
            {
                if (this.chatHubService.IsBlacklisted(room, user))
                {
                    throw new HubException("You have been added to blacklist for this room.");
                }
            }

            if (room.Protected())
            {
                if (!Context.User.Identity.IsAuthenticated)
                {
                    throw new HubException("This room is for authenticated user only.");
                }
            }

            if (room.Private())
            {
                if (!this.chatHubService.IsWhitelisted(room, user))
                {
                    throw new HubException("No valid private room connection.");
                }
            }

            if (room.OneVsOne())
            {
                if (!this.chatHubService.IsValidOneVsOneConnection(room, user))
                {
                    throw new HubException("No valid one vs one room id.");
                }
            }

            if (room.Public() || room.Protected() || room.Private() || room.OneVsOne())
            {
                this.chatHubRepository.DeleteChatHubRoomChatHubUser(roomId, user.UserId);
                ChatHubRoom chatHubRoomClientModel = await this.chatHubService.CreateChatHubRoomClientModelAsync(room);

                foreach (var connection in user.Connections.Active())
                {
                    await Groups.RemoveFromGroupAsync(connection.ConnectionId, room.Id.ToString());
                    await Clients.Client(connection.ConnectionId).SendAsync("RemoveRoom", chatHubRoomClientModel);
                }

                ChatHubUser chatHubUserClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
                await Clients.Group(room.Id.ToString()).SendAsync("RemoveUser", chatHubUserClientModel, room.Id.ToString());
                await this.SendGroupNotification(string.Format("{0} left chat room with client device {1}.", user.DisplayName, this.chatHubService.MakeStringAnonymous(Context.ConnectionId, 7, '*')), room.Id, Context.ConnectionId, user, ChatHubMessageType.Enter_Leave);
            }
        }

        [AllowAnonymous]
        public async Task SendMessage(string message, int roomId, int moduleId)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            if (await ExecuteCommandManager(user, message, roomId))
            {
                return;
            }

            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = user.UserId,
                User = user,
                Content = message ?? string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), ChatHubMessageType.Guest)
            };
            this.chatHubRepository.AddChatHubMessage(chatHubMessage);

            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage);
            var connectionsIds = this.chatHubService.GetAllExceptConnectionIds(user);
            await Clients.GroupExcept(roomId.ToString(), connectionsIds).SendAsync("AddMessage", chatHubMessageClientModel);
        }
        private async Task<bool> ExecuteCommandManager(ChatHubUser chatHubUser, string message, int roomId)
        {
            var commandManager = new CommandManager(Context.ConnectionId, roomId, chatHubUser, this, chatHubService, chatHubRepository, userManager);
            return await commandManager.TryHandleCommand(message);
        }
        [AllowAnonymous]
        public async Task SendCommandMetaDatas(int roomId)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();
            List<string> callerUserRoles = new List<string>() { RoleNames.Everyone };

            if (Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
            {
                callerUserRoles.Add(RoleNames.Admin);
            }

            List<ChatHubCommandMetaData> commandMetaDatas = CommandManager.GetCommandsMetaDataByUserRole(callerUserRoles.ToArray()).ToList();

            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = user.UserId,
                User = user,
                Content = string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), ChatHubMessageType.Commands),
                CommandMetaDatas = commandMetaDatas
            };
            this.chatHubRepository.AddChatHubMessage(chatHubMessage);

            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage);
            await Clients.Clients(user.Connections.Active().Select(c => c.ConnectionId).ToArray<string>()).SendAsync("AddMessage", chatHubMessageClientModel);
        }

        public async Task SendClientNotification(string message, int roomId, string connectionId, ChatHubUser targetUser, ChatHubMessageType chatHubMessageType)
        {
            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = targetUser.UserId,
                User = targetUser,
                Content = message ?? string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), chatHubMessageType)
            };
            this.chatHubRepository.AddChatHubMessage(chatHubMessage);

            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage);
            await Clients.Client(connectionId).SendAsync("AddMessage", chatHubMessageClientModel);
        }
        public async Task SendGroupNotification(string message, int roomId, string connectionId, ChatHubUser contextUser, ChatHubMessageType chatHubMessageType)
        {
            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = roomId,
                ChatHubUserId = contextUser.UserId,
                User = contextUser,
                Content = message ?? string.Empty,
                Type = Enum.GetName(typeof(ChatHubMessageType), chatHubMessageType)
            };
            this.chatHubRepository.AddChatHubMessage(chatHubMessage);

            ChatHubMessage chatHubMessageClientModel = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage);
            var connectionsIds = this.chatHubService.GetAllExceptConnectionIds(contextUser);
            await Clients.GroupExcept(roomId.ToString(), connectionsIds).SendAsync("AddMessage", chatHubMessageClientModel);
        }

        [AllowAnonymous]
        public async Task UploadBytes(IAsyncEnumerable<string> dataURI, string roomId)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();
            var connectionsIds = this.chatHubService.GetAllExceptConnectionIds(user);

            foreach (var connection in user.Connections)
            {
                connectionsIds.Add(connection.ConnectionId);
            }

            string dataURIresult = string.Empty;
            IAsyncEnumerator<string> enumerators = dataURI.GetAsyncEnumerator();

            while (await enumerators.MoveNextAsync())
            {
                dataURIresult += enumerators.Current;
            }

            await Clients.GroupExcept(roomId, connectionsIds).SendAsync("DownloadBytes", dataURIresult, roomId);
        }

        [AllowAnonymous]
        public async Task<List<ChatHubUser>> GetIgnoredUsers()
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            IQueryable<ChatHubUser> ignoredUsers = null;
            if (user != null)
            {
                ignoredUsers = this.chatHubRepository.GetIgnoredApplicationUsers(user);
            }

            if (ignoredUsers != null && ignoredUsers.Any())
            {
                List<ChatHubUser> chatHubUserClientModel = new List<ChatHubUser>();
                foreach (var ignoredUser in ignoredUsers)
                {
                    ChatHubUser clientModel = this.chatHubService.CreateChatHubUserClientModel(ignoredUser);
                    chatHubUserClientModel.Add(clientModel);
                }

                return chatHubUserClientModel;
            }

            return null;
        }
        [AllowAnonymous]
        public async Task<List<ChatHubUser>> GetIgnoredByUsers()
        {
            ChatHubUser user = await this.GetChatHubUserAsync();

            IQueryable<ChatHubIgnore> ignoredByUsers = null;
            if (user != null)
            {
                ignoredByUsers = this.chatHubRepository.GetIgnoredByUsers(user);
            }

            IQueryable<ChatHubUser> chatHubUserClientModels = ignoredByUsers.Select(x =>

                this.chatHubService.CreateChatHubUserClientModel(x.User)
            );

            var list = await chatHubUserClientModels.ToListAsync();
            return list;
        }

        [AllowAnonymous]
        public async Task IgnoreUser(string username)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();
            ChatHubUser targetUser = await this.chatHubRepository.GetUserByUserNameAsync(username);

            if (user != null && targetUser != null)
            {
                if (user == targetUser)
                {
                    throw new HubException("Calling user cannot be target user.");
                }

                this.chatHubService.IgnoreUser(user, targetUser);

                var targetUserClientModel = this.chatHubService.CreateChatHubUserClientModel(targetUser);
                foreach (var connection in user.Connections.Active())
                {
                    await Clients.Client(connection.ConnectionId).SendAsync("AddIgnoredUser", targetUserClientModel);
                }

                var userClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
                foreach (var connection in targetUser.Connections.Active())
                {
                    await Clients.Client(connection.ConnectionId).SendAsync("AddIgnoredByUser", userClientModel);
                }
            }
        }
        [AllowAnonymous]
        public async Task UnignoreUser(string username)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();
            ChatHubUser targetUser = await this.chatHubRepository.GetUserByUserNameAsync(username);

            if (user != null && targetUser != null)
            {
                ChatHubIgnore chatHubIgnore = this.chatHubRepository.GetChatHubIgnore(user.UserId, targetUser.UserId);

                if (chatHubIgnore != null)
                {
                    this.chatHubRepository.DeleteChatHubIgnore(chatHubIgnore);

                    var targetUserClientModel = this.chatHubService.CreateChatHubUserClientModel(targetUser);
                    foreach (var connection in user.Connections.Active())
                    {
                        await Clients.Client(connection.ConnectionId).SendAsync("RemoveIgnoredUser", targetUserClientModel);
                    }

                    var userClientModel = this.chatHubService.CreateChatHubUserClientModel(user);
                    foreach (var connection in targetUser.Connections.Active())
                    {
                        await Clients.Client(connection.ConnectionId).SendAsync("RemoveIgnoredByUser", userClientModel);
                    }
                }
            }
        }

        [AllowAnonymous]
        public async Task AddModerator(int userId, int roomId)
        {
            try
            {
                ChatHubUser user = await this.GetChatHubUserAsync();
                ChatHubUser targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);
            
                if (user != null && targetUser != null)
                {
                    var room = this.chatHubRepository.GetChatHubRoom(roomId);
                    if (user.UserId != room.CreatorId && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators and administrators can add moderations.");
                    }

                    ChatHubModerator moderator = this.chatHubRepository.GetChatHubModerator(targetUser.UserId);
                    if(moderator == null)
                    {
                        moderator = new ChatHubModerator()
                        {
                            ChatHubUserId = targetUser.UserId,
                            ModeratorDisplayName = targetUser.DisplayName
                        };
                        moderator = this.chatHubRepository.AddChatHubModerator(moderator);
                    }

                    ChatHubRoomChatHubModerator room_moderator = new ChatHubRoomChatHubModerator()
                    {
                        ChatHubRoomId = roomId,
                        ChatHubModeratorId = moderator.Id,
                    };
                    this.chatHubRepository.AddChatHubRoomChatHubModerator(room_moderator);

                    var targetModeratorClientModel = this.chatHubService.CreateChatHubModeratorClientModel(moderator);
                    await Clients.Group(roomId.ToString()).SendAsync("AddModerator", targetModeratorClientModel, roomId);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }
        [AllowAnonymous]
        public async Task RemoveModerator(int userId, int roomId)
        {
            try
            {
                ChatHubUser user = await this.GetChatHubUserAsync();
                ChatHubUser targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);
            
                if (user != null && targetUser != null)
                {
                    var room = this.chatHubRepository.GetChatHubRoom(roomId);
                    if (user.UserId != room.CreatorId && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators and administrators can remove moderations.");
                    }

                    var moderator = this.chatHubRepository.GetChatHubModerator(targetUser.UserId);
                    this.chatHubRepository.DeleteChatHubRoomChatHubModerator(roomId, moderator.Id);

                    var targetModeratorClientModel = this.chatHubService.CreateChatHubModeratorClientModel(moderator);
                    await Clients.Group(roomId.ToString()).SendAsync("RemoveModerator", targetModeratorClientModel, roomId);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }
        [AllowAnonymous]
        public async Task AddWhitelistUser(int userId, int roomId)
        {
            try
            {
                ChatHubUser user = await this.GetChatHubUserAsync();
                ChatHubUser targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);

                if (user != null && targetUser != null)
                {
                    var room = this.chatHubRepository.GetChatHubRoom(roomId);
                    if (user.UserId != room.CreatorId && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators and administrators can add whitelist users.");
                    }

                    ChatHubWhitelistUser whitelistUser = this.chatHubRepository.AddChatHubWhitelistUser(targetUser);

                    ChatHubRoomChatHubWhitelistUser room_whitelistuser = new ChatHubRoomChatHubWhitelistUser()
                    {
                        ChatHubRoomId = roomId,
                        ChatHubWhitelistUserId = whitelistUser.Id,
                    };
                    this.chatHubRepository.AddChatHubRoomChatHubWhitelistUser(room_whitelistuser);

                    var targetWhitelistUserClientModel = this.chatHubService.CreateChatHubWhitelistUserClientModel(whitelistUser);
                    await Clients.Group(roomId.ToString()).SendAsync("AddWhitelistUser", targetWhitelistUserClientModel, roomId);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }
        [AllowAnonymous]
        public async Task RemoveWhitelistUser(int userId, int roomId)
        {
            try
            {
                ChatHubUser user = await this.GetChatHubUserAsync();
                ChatHubUser targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);

                if (user != null && targetUser != null)
                {
                    var room = this.chatHubRepository.GetChatHubRoom(roomId);
                    if (user.UserId != room.CreatorId && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators and administrators can remove whitelist users.");
                    }

                    var whitelistUser = this.chatHubRepository.GetChatHubWhitelistUser(targetUser.UserId);
                    this.chatHubRepository.DeleteChatHubRoomChatHubWhitelistUser(roomId, whitelistUser.Id);

                    var targetWhitelistUserClientModel = this.chatHubService.CreateChatHubWhitelistUserClientModel(whitelistUser);
                    await Clients.Group(roomId.ToString()).SendAsync("RemoveWhitelistUser", targetWhitelistUserClientModel, roomId);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }
        [AllowAnonymous]
        public async Task AddBlacklistUser(int userId, int roomId)
        {
            try
            {
                ChatHubUser user = await this.GetChatHubUserAsync();
                ChatHubUser targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);

                if (user != null && targetUser != null)
                {
                    var room = this.chatHubRepository.GetChatHubRoom(roomId);
                    if (user.UserId != room.CreatorId && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators and administrators can add blacklist users.");
                    }

                    ChatHubBlacklistUser blacklistUser = this.chatHubRepository.AddChatHubBlacklistUser(targetUser);

                    ChatHubRoomChatHubBlacklistUser room_blacklistuser = new ChatHubRoomChatHubBlacklistUser()
                    {
                        ChatHubRoomId = roomId,
                        ChatHubBlacklistUserId = blacklistUser.Id,
                    };
                    this.chatHubRepository.AddChatHubRoomChatHubBlacklistUser(room_blacklistuser);

                    var targetBlacklistUserClientModel = this.chatHubService.CreateChatHubBlacklistUserClientModel(blacklistUser);
                    await Clients.Group(roomId.ToString()).SendAsync("AddBlacklistUser", targetBlacklistUserClientModel, roomId);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }
        [AllowAnonymous]
        public async Task RemoveBlacklistUser(int userId, int roomId)
        {
            try
            {
                ChatHubUser user = await this.GetChatHubUserAsync();
                ChatHubUser targetUser = await this.chatHubRepository.GetUserByIdAsync(userId);

                if (user != null && targetUser != null)
                {
                    var room = this.chatHubRepository.GetChatHubRoom(roomId);
                    if (user.UserId != room.CreatorId && !Context.User.HasClaim(ClaimTypes.Role, RoleNames.Admin))
                    {
                        throw new HubException("Only room creators and administrators can remove blacklis users.");
                    }

                    var blacklistUser = this.chatHubRepository.GetChatHubBlacklistUser(targetUser.UserId);
                    this.chatHubRepository.DeleteChatHubRoomChatHubBlacklistUser(roomId, blacklistUser.Id);

                    var targetBlacklistUserClientModel = this.chatHubService.CreateChatHubBlacklistUserClientModel(blacklistUser);
                    await Clients.Group(roomId.ToString()).SendAsync("RemoveBlacklistUser", targetBlacklistUserClientModel, roomId);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }

        private async Task AddWaitingRoomItem(ChatHubUser user, ChatHubRoom room)
        {
            var chatHubWaitingRoomItem = new ChatHubWaitingRoomItem()
            {
                Guid = Guid.NewGuid(),
                RoomId = room.Id,
                UserId = user.UserId,
                DisplayName = user.DisplayName
            };

            var roomCreator = await this.chatHubRepository.GetUserByIdAsync(room.CreatorId);
            foreach (var connection in roomCreator.Connections.Active())
            {
                await Clients.Client(connection.ConnectionId).SendAsync("AddWaitingRoomItem", chatHubWaitingRoomItem);
            }
        }
        [AllowAnonymous]
        public async Task RemoveWaitingRoomItem(ChatHubWaitingRoomItem waitingRoomItem)
        {
            ChatHubUser user = await this.GetChatHubUserAsync();
            ChatHubUser targetUser = await this.chatHubRepository.GetUserByIdAsync(waitingRoomItem.UserId);

            await Clients.Clients(targetUser.Connections.Active().Select(item => item.ConnectionId)).SendAsync("RemovedWaitingRoomItem", waitingRoomItem);            
        }
        
        private string CreateUsername(string guestname)
        {
            string base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            string id = Regex.Replace(base64Guid, "[/+=]", "");
            string userName = string.Concat(guestname, "-", new Random().Next(1000, 9999), "-", id);

            return userName;
        }
        private string CreateDisplaynameFromUsername(string username)
        {
            var name = username.Substring(0, username.IndexOf('-'));
            var numbers = username.Substring(username.IndexOf('-') + 1, 4);
            var displayname = string.Concat(name, "-", numbers);
            return displayname;
        }
        private bool IsValidGuestUsername(string guestName)
        {
            string guestNamePattern = "^([a-zA-Z0-9_]{3,32})$";
            Regex regex = new Regex(guestNamePattern);
            Match match = regex.Match(guestName);
            return match.Success;
        }
        public string CreateSignalRChatHubRoomGroupLevelName(string roomId, int userRoomLevel)
        {
            return $"roomId:{roomId};userRoomLevel:{userRoomLevel};";
        }

    }

}