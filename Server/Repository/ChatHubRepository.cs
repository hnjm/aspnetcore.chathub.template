using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Modules;
using System;
using System.Threading.Tasks;
using Oqtane.Shared.Enums;
using Oqtane.Shared.Models;
using Oqtane.Models;

namespace Oqtane.ChatHubs.Repository
{
    public class ChatHubRepository : IChatHubRepository, IService
    {

        private readonly ChatHubContext db;

        public ChatHubRepository(ChatHubContext dbContext)
        {
            this.db = dbContext;
        }

        #region GET

        public IQueryable<ChatHubRoom> GetChatHubRooms()
        {
            try
            {
                return db.ChatHubRoom.Select(item => item);
            }
            catch
            {
                throw;
            }
        }
        public IQueryable<ChatHubRoom> GetChatHubRoomsByModuleId(int ModuleId)
        {
            try
            {
                return db.ChatHubRoom.Where(item => item.ModuleId == ModuleId);
            }
            catch
            {
                throw;
            }
        }
        public IQueryable<ChatHubRoom> GetChatHubRoomsByUser(ChatHubUser user)
        {
            return db.Entry(user)
                      .Collection(u => u.UserRooms)
                      .Query().Select(u => u.Room);            
        }
        public IQueryable<ChatHubRoom> GetChatHubRoomsByCreator(int creatorId)
        {
            return db.ChatHubRoom.Where(item => item.CreatorId == creatorId);
        }
        public IQueryable<ChatHubUser> GetChatHubUsersByRoom(ChatHubRoom room)
        {
            return db.Entry(room)
                      .Collection(r => r.RoomUsers)
                      .Query().Select(r => r.User);
        }
        public ChatHubRoom GetChatHubRoom(int ChatHubRoomId)
        {
            try
            {
                return db.ChatHubRoom.FirstOrDefault(room => room.Id == ChatHubRoomId);
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoom GetChatHubRoomOneVsOne(string OneVsOneId)
        {
            try
            {
                return db.ChatHubRoom.FirstOrDefault(item => item.OneVsOneId == OneVsOneId);
            }
            catch
            {
                throw;
            }
        }
        public IList<ChatHubMessage> GetChatHubMessages(int roomId, int count)
        {
            try
            {
                // for now return list instead of queryable result
                // return db.ChatHubMessage.Where(item => item.ChatHubRoomId == roomId).Include(item => item.User).OrderByDescending(item => item.CreatedOn);

                var msgs = db.ChatHubMessage.Where(item => item.ChatHubRoomId == roomId).OrderByDescending(item => item.CreatedOn).Take(count).ToList();
                foreach (var msg in msgs)
                {
                    msg.User = db.ChatHubUser.FirstOrDefault(item => item.UserId == msg.ChatHubUserId);
                }

                return msgs;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubMessage GetChatHubMessage(int ChatHubMessageId)
        {
            try
            {
                return db.ChatHubMessage.Where(item => item.Id == ChatHubMessageId).Include(item => item.User).Include(item => item.Photos).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }
        public IQueryable<ChatHubUser> GetOnlineUsers()
        {
            return db.ChatHubUser.Include(u => u.Connections).Where(u => u.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)));
        }
        public IQueryable<ChatHubUser> GetOnlineUsers(int roomId)
        {
            var query = from user in db.ChatHubUser
                        join room_user in db.ChatHubRoomChatHubUser
                            on user.UserId equals room_user.ChatHubUserId
                        where room_user.ChatHubRoomId == roomId
                        join connection in db.ChatHubConnection
                            on user.UserId equals connection.ChatHubUserId
                        where connection.Status == ChatHubConnectionStatus.Active.ToString()
                        select user;

            return query;
        }
        public IQueryable<ChatHubConnection> GetConnectionsByUserId(int userId)
        {
            ChatHubUser user = db.ChatHubUser.Where(u => u.UserId == userId).Include(u => u.Connections).FirstOrDefault();

            return db.Entry(user)
                      .Collection(u => u.Connections)
                      .Query().Select(c => c);
        }
        public async Task<ChatHubConnection> GetConnectionByConnectionId(string connectionId)
        {
            return await db.ChatHubConnection
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ConnectionId == connectionId);
        }
        public ChatHubRoomChatHubUser GetChatHubRoomChatHubUser(int chatHubRoomId, int chatHubUserId)
        {
            return db.ChatHubRoomChatHubUser
                    .Where(item => item.ChatHubRoomId == chatHubRoomId)
                    .Where(item => item.ChatHubUserId == chatHubUserId)
                    .FirstOrDefault();
        }
        public ChatHubIgnore GetChatHubIgnore(int callerUserId, int targetUserId)
        {
            return this.db.ChatHubIgnore.FirstOrDefault(item => item.ChatHubUserId == callerUserId && item.ChatHubIgnoredUserId == targetUserId);
        }
        public IQueryable<ChatHubIgnore> GetIgnoredUsers(ChatHubUser user)
        {
            return db.Entry(user)
                      .Collection(u => u.Ignores)
                      .Query().Select(i => i);
        }
        public IQueryable<ChatHubUser> GetIgnoredApplicationUsers(ChatHubUser user)
        {
            IQueryable<int> chatIgnoreIdList = db.Entry(user)
                      .Collection(u => u.Ignores)
                      .Query()
                      .Where(i => (i.ModifiedOn.AddDays(7)) >= DateTime.Now)
                      .Select(i => i.ChatHubIgnoredUserId);

            return db.ChatHubUser.Where(u => chatIgnoreIdList.Contains(u.UserId)).Include(u => u.Connections);
        }
        public IQueryable<ChatHubUser> GetIgnoredByApplicationUsers(ChatHubUser user)
        {
            IQueryable<int> chatIgnoredByIdList = db.ChatHubIgnore
                      .Include(i => i.User)
                      .Where(i => i.ChatHubIgnoredUserId == user.UserId && (i.ModifiedOn.AddDays(7)) >= DateTime.Now)
                      .Select(i => i.ChatHubUserId);

            return db.ChatHubUser.Where(u => chatIgnoredByIdList.Contains(u.UserId)).Include(u => u.Connections);
        }
        public IQueryable<ChatHubIgnore> GetIgnoredByUsers(ChatHubUser user)
        {
            return db.ChatHubIgnore.Include(i => i.User).Select(i => i).Where(i => i.Id == user.UserId && (i.ModifiedOn.AddDays(7)) >= DateTime.Now);
        }
        public ChatHubSettings GetChatHubSetting(int ChatHubUserId)
        {
            try
            {
                return db.ChatHubSetting.Include(item => item.User).Where(item => item.ChatHubUserId == ChatHubUserId).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }
        public ChatHubSettings GetChatHubSettingByUser(ChatHubUser user)
        {
            try
            {
                return db.ChatHubSetting.Where(item => item.ChatHubUserId == user.UserId).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }
        public async Task<ChatHubUser> GetUserByIdAsync(int id)
        {
            var item = await db.ChatHubUser
                            .Include(u => u.Connections)
                            .Include(u => u.Settings)
                            .Include(u => u.UserRooms)
                            .Where(u => u.UserId == id)
                            .Select(u => new
                            {
                                User = u,
                                Connections = u.Connections.Where(c => c.Status == ChatHubConnectionStatus.Active.ToString()).ToList(),
                            })
                            .FirstOrDefaultAsync();

            if (item != null)
            {
                ChatHubUser user = item.User;
                user.Connections = item.Connections;

                return user;
            }

            return null;
        }
        public async Task<ChatHubUser> GetUserByUserNameAsync(string username)
        {
            var item = await db.ChatHubUser
                            .Include(u => u.Connections)
                            .Where(u => u.Username == username)
                            .Select(u => new
                            {
                                User = u,
                                Connections = u.Connections.Where(c => c.Status == ChatHubConnectionStatus.Active.ToString()).ToList(),
                            })
                            .FirstOrDefaultAsync();

            if (item != null)
            {
                ChatHubUser user = item.User;
                user.Connections = item.Connections;

                return user;
            }

            return null;
        }
        public async Task<ChatHubUser> GetUserByDisplayName(string displayName)
        {
            ChatHubUser user = await db.ChatHubUser.Include(u => u.Connections).Where(u => u.DisplayName == displayName).Where(u => u.Connections.Any(c => c.Status == ChatHubConnectionStatus.Active.ToString())).FirstOrDefaultAsync();
            return user;
        }
        public ChatHubModerator GetChatHubModerator(int ChatHubUserId)
        {
            try
            {
                return db.ChatHubModerator.FirstOrDefault(item => item.ChatHubUserId == ChatHubUserId);
            }
            catch
            {
                throw;
            }
        }
        public IQueryable<ChatHubModerator> GetChatHubModerators(ChatHubRoom ChatHubRoom)
        {
            try
            {
                return db.Entry(ChatHubRoom)
                      .Collection(item => item.RoomModerators)
                      .Query().Select(item => item.Moderator);
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoomChatHubModerator GetChatHubRoomChatHubModerator(int chatHubRoomId, int chatHubModeratorId)
        {
            return db.ChatHubRoomChatHubModerator
                    .Where(item => item.ChatHubRoomId == chatHubRoomId)
                    .Where(item => item.ChatHubModeratorId == chatHubModeratorId)
                    .FirstOrDefault();
        }
        public ChatHubWhitelistUser GetChatHubWhitelistUser(int ChatHubUserId)
        {
            try
            {
                return db.ChatHubWhitelistUser.FirstOrDefault(item => item.ChatHubUserId == ChatHubUserId);
            }
            catch
            {
                throw;
            }
        }
        public IQueryable<ChatHubWhitelistUser> GetChatHubWhitelistUsers(ChatHubRoom ChatHubRoom)
        {
            try
            {
                return db.Entry(ChatHubRoom)
                      .Collection(item => item.RoomWhitelistUsers)
                      .Query().Select(item => item.WhitelistUser);
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoomChatHubWhitelistUser GetChatHubRoomChatHubWhitelistUser(int chatHubRoomId, int chatHubWhitelistUserId)
        {
            return db.ChatHubRoomChatHubWhitelistUser
                    .Where(item => item.ChatHubRoomId == chatHubRoomId)
                    .Where(item => item.ChatHubWhitelistUserId == chatHubWhitelistUserId)
                    .FirstOrDefault();
        }
        public ChatHubBlacklistUser GetChatHubBlacklistUser(int ChatHubUserId)
        {
            try
            {
                return db.ChatHubBlacklistUser.FirstOrDefault(item => item.ChatHubUserId == ChatHubUserId);
            }
            catch
            {
                throw;
            }
        }
        public IQueryable<ChatHubBlacklistUser> GetChatHubBlacklistUsers(ChatHubRoom ChatHubRoom)
        {
            try
            {
                return db.Entry(ChatHubRoom)
                      .Collection(item => item.RoomBlacklistUsers)
                      .Query().Select(item => item.BlacklistUser);
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoomChatHubBlacklistUser GetChatHubRoomChatHubBlacklistUser(int chatHubRoomId, int chatHubBlacklistUserId)
        {
            return db.ChatHubRoomChatHubBlacklistUser
                    .Where(item => item.ChatHubRoomId == chatHubRoomId)
                    .Where(item => item.ChatHubBlacklistUserId == chatHubBlacklistUserId)
                    .FirstOrDefault();
        }

        #endregion

        #region ADD

        public ChatHubRoom AddChatHubRoom(ChatHubRoom ChatHubRoom)
        {
            try
            {
                db.ChatHubRoom.Add(ChatHubRoom);
                db.SaveChanges();
                return ChatHubRoom;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubMessage AddChatHubMessage(ChatHubMessage ChatHubMessage)
        {
            try
            {
                db.ChatHubMessage.Add(ChatHubMessage);
                db.SaveChanges();
                return ChatHubMessage;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubConnection AddChatHubConnection(ChatHubConnection ChatHubConnection)
        {
            try
            {
                db.ChatHubConnection.Add(ChatHubConnection);
                db.SaveChanges();
                return ChatHubConnection;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubUser AddChatHubUser(ChatHubUser ChatHubUser)
        {
            try
            {
                db.ChatHubUser.Add(ChatHubUser);
                db.SaveChanges();
                return ChatHubUser;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoomChatHubUser AddChatHubRoomChatHubUser(ChatHubRoomChatHubUser ChatHubRoomChatHubUser)
        {
            try
            {
                var item = this.GetChatHubRoomChatHubUser(ChatHubRoomChatHubUser.ChatHubRoomId, ChatHubRoomChatHubUser.ChatHubUserId);
                if (item == null)
                {
                    db.ChatHubRoomChatHubUser.Add(ChatHubRoomChatHubUser);
                    db.SaveChanges();
                    return ChatHubRoomChatHubUser;
                }

                return item;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubPhoto AddChatHubPhoto(ChatHubPhoto ChatHubPhoto)
        {
            try
            {
                db.ChatHubMessage.Attach(ChatHubPhoto.Message);
                db.Entry(ChatHubPhoto.Message).State = EntityState.Modified;

                db.ChatHubPhoto.Add(ChatHubPhoto);
                db.SaveChanges();
                return ChatHubPhoto;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubIgnore AddChatHubIgnore(ChatHubIgnore chatHubIgnore)
        {
            try
            {
                db.ChatHubUser.Attach(chatHubIgnore.User);
                db.Entry(chatHubIgnore.User).State = EntityState.Modified;

                db.ChatHubIgnore.Add(chatHubIgnore);
                db.SaveChanges();
                return chatHubIgnore;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubSettings AddChatHubSetting(ChatHubSettings ChatHubSetting)
        {
            try
            {
                db.ChatHubSetting.Add(ChatHubSetting);
                db.SaveChanges();
                return ChatHubSetting;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubModerator AddChatHubModerator(ChatHubModerator ChatHubModerator)
        {
            try
            {
                db.ChatHubModerator.Add(ChatHubModerator);
                db.SaveChanges();
                return ChatHubModerator;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoomChatHubModerator AddChatHubRoomChatHubModerator(ChatHubRoomChatHubModerator ChatHubRoomChatHubModerator)
        {
            try
            {
                var item = this.GetChatHubRoomChatHubModerator(ChatHubRoomChatHubModerator.ChatHubRoomId, ChatHubRoomChatHubModerator.ChatHubModeratorId);
                if (item == null)
                {
                    db.ChatHubRoomChatHubModerator.Add(ChatHubRoomChatHubModerator);
                    db.SaveChanges();
                    return ChatHubRoomChatHubModerator;
                }

                return item;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubWhitelistUser AddChatHubWhitelistUser(ChatHubUser targetUser)
        {
            try
            {
                ChatHubWhitelistUser ChatHubWhitelistUser = this.GetChatHubWhitelistUser(targetUser.UserId);
                if(ChatHubWhitelistUser == null)
                {
                    ChatHubWhitelistUser = new ChatHubWhitelistUser()
                    {
                        ChatHubUserId = targetUser.UserId,
                        WhitelistUserDisplayName = targetUser.DisplayName,
                    };

                    db.ChatHubWhitelistUser.Add(ChatHubWhitelistUser);
                    db.SaveChanges();
                }
                return ChatHubWhitelistUser;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoomChatHubWhitelistUser AddChatHubRoomChatHubWhitelistUser(ChatHubRoomChatHubWhitelistUser ChatHubRoomChatHubWhitelistUser)
        {
            try
            {
                var item = this.GetChatHubRoomChatHubWhitelistUser(ChatHubRoomChatHubWhitelistUser.ChatHubRoomId, ChatHubRoomChatHubWhitelistUser.ChatHubWhitelistUserId);
                if (item == null)
                {
                    db.ChatHubRoomChatHubWhitelistUser.Add(ChatHubRoomChatHubWhitelistUser);
                    db.SaveChanges();
                    return ChatHubRoomChatHubWhitelistUser;
                }

                return item;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubBlacklistUser AddChatHubBlacklistUser(ChatHubUser targetUser)
        {
            try
            {
                ChatHubBlacklistUser ChatHubBlacklistUser = this.GetChatHubBlacklistUser(targetUser.UserId);
                if (ChatHubBlacklistUser == null)
                {
                    ChatHubBlacklistUser = new ChatHubBlacklistUser()
                    {
                        ChatHubUserId = targetUser.UserId,
                        BlacklistUserDisplayName = targetUser.DisplayName,
                    };

                    db.ChatHubBlacklistUser.Add(ChatHubBlacklistUser);
                    db.SaveChanges();
                }
                return ChatHubBlacklistUser;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoomChatHubBlacklistUser AddChatHubRoomChatHubBlacklistUser(ChatHubRoomChatHubBlacklistUser ChatHubRoomChatHubBlacklistUser)
        {
            try
            {
                var item = this.GetChatHubRoomChatHubBlacklistUser(ChatHubRoomChatHubBlacklistUser.ChatHubRoomId, ChatHubRoomChatHubBlacklistUser.ChatHubBlacklistUserId);
                if (item == null)
                {
                    db.ChatHubRoomChatHubBlacklistUser.Add(ChatHubRoomChatHubBlacklistUser);
                    db.SaveChanges();
                    return ChatHubRoomChatHubBlacklistUser;
                }

                return item;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region ADD OR UPDATE

        #endregion

        #region DELETE

        public void DeleteChatHubUser(int UserId)
        {
            try
            {
                ChatHubUser ChatHubUser = db.ChatHubUser.Where(item => item.UserId == UserId).FirstOrDefault();
                db.ChatHubUser.Remove(ChatHubUser);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubRoom(int ChatHubRoomId, int ModuleId)
        {
            try
            {
                ChatHubRoom ChatHubRoom = db.ChatHubRoom.Where(item => item.Id == ChatHubRoomId)
                    .Where(item => item.ModuleId == ModuleId).FirstOrDefault();
                db.ChatHubRoom.Remove(ChatHubRoom);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubRooms(int userId, int ModuleId)
        {
            try
            {
                IQueryable<ChatHubRoom> rooms = db.ChatHubRoom.Where(item => item.CreatorId == userId)
                    .Where(item => item.ModuleId == ModuleId);
                db.ChatHubRoom.RemoveRange(rooms);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubMessage(int ChatHubMessageId, int ChatHubRoomId)
        {
            try
            {
                ChatHubMessage ChatHubMessage = db.ChatHubMessage.Where(item => item.Id == ChatHubMessageId)
                    .Where(item => item.ChatHubRoomId == ChatHubRoomId).FirstOrDefault();
                db.ChatHubMessage.Remove(ChatHubMessage);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubMessages(int userId)
        {
            try
            {
                IQueryable<ChatHubMessage> messages = db.ChatHubMessage.Where(item => item.ChatHubUserId == userId);
                db.ChatHubMessage.RemoveRange(messages);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubConnection(int ChatHubConnectionId, int ChatHubUserId)
        {
            try
            {
                ChatHubConnection ChatHubConnection = db.ChatHubConnection.Where(item => item.Id == ChatHubConnectionId)
                    .Where(item => item.ChatHubUserId == ChatHubUserId).FirstOrDefault();
                db.ChatHubConnection.Remove(ChatHubConnection);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubConnections(int userId)
        {
            try
            {
                IQueryable<ChatHubConnection> connections = db.ChatHubConnection.Where(item => item.ChatHubUserId == userId);
                db.ChatHubConnection.RemoveRange(connections);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubRoomChatHubUser(int ChatHubRoomId, int ChatHubUserId)
        {
            try
            {
                ChatHubRoomChatHubUser item = this.GetChatHubRoomChatHubUser(ChatHubRoomId, ChatHubUserId);

                if (item != null)
                {
                    db.ChatHubRoomChatHubUser.Remove(item);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubIgnore(ChatHubIgnore chatHubIgnore)
        {
            try
            {
                db.ChatHubIgnore.Remove(chatHubIgnore);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubModerator(int ModeratorId)
        {
            try
            {
                ChatHubModerator ChatHubModerator = db.ChatHubModerator.Where(item => item.Id == ModeratorId).FirstOrDefault();
                db.ChatHubModerator.Remove(ChatHubModerator);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubRoomChatHubModerator(int ChatHubRoomId, int ChatHubModeratorId)
        {
            try
            {
                ChatHubRoomChatHubModerator item = this.GetChatHubRoomChatHubModerator(ChatHubRoomId, ChatHubModeratorId);
                if (item != null)
                {
                    db.ChatHubRoomChatHubModerator.Remove(item);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubWhitelistUser(int WhitelistUserId)
        {
            try
            {
                ChatHubWhitelistUser ChatHubWhitelistUser = db.ChatHubWhitelistUser.Where(item => item.Id == WhitelistUserId).FirstOrDefault();
                db.ChatHubWhitelistUser.Remove(ChatHubWhitelistUser);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubRoomChatHubWhitelistUser(int ChatHubRoomId, int ChatHubWhitelistUserId)
        {
            try
            {
                ChatHubRoomChatHubWhitelistUser item = this.GetChatHubRoomChatHubWhitelistUser(ChatHubRoomId, ChatHubWhitelistUserId);
                if (item != null)
                {
                    db.ChatHubRoomChatHubWhitelistUser.Remove(item);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubBlacklistUser(int BlacklistUserId)
        {
            try
            {
                ChatHubBlacklistUser ChatHubBlacklistUser = db.ChatHubBlacklistUser.Where(item => item.Id == BlacklistUserId).FirstOrDefault();
                db.ChatHubBlacklistUser.Remove(ChatHubBlacklistUser);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubRoomChatHubBlacklistUser(int ChatHubRoomId, int ChatHubBlacklistUserId)
        {
            try
            {
                ChatHubRoomChatHubBlacklistUser item = this.GetChatHubRoomChatHubBlacklistUser(ChatHubRoomId, ChatHubBlacklistUserId);
                if (item != null)
                {
                    db.ChatHubRoomChatHubBlacklistUser.Remove(item);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region UPDATE

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                // TODO: remove oqtane framework weird workarrounds with discriminators
                await db.Database.ExecuteSqlRawAsync($"UPDATE [dbo].[User] SET UserType='ChatHubUser' WHERE UserId={user.UserId}");
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoom UpdateChatHubRoom(ChatHubRoom ChatHubRoom)
        {
            try
            {
                db.Entry(ChatHubRoom).State = EntityState.Modified;
                db.SaveChanges();
                return ChatHubRoom;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubMessage UpdateChatHubMessage(ChatHubMessage ChatHubMessage)
        {
            try
            {
                db.Entry(ChatHubMessage).State = EntityState.Modified;
                db.SaveChanges();
                return ChatHubMessage;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubConnection UpdateChatHubConnection(ChatHubConnection ChatHubConnection)
        {
            try
            {
                db.Entry(ChatHubConnection).State = EntityState.Modified;
                db.SaveChanges();
                return ChatHubConnection;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubIgnore UpdateChatHubIgnore(ChatHubIgnore chatHubIgnore)
        {
            try
            {
                db.Entry(chatHubIgnore).State = EntityState.Modified;
                db.SaveChanges();
                return chatHubIgnore;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubSettings UpdateChatHubSetting(ChatHubSettings ChatHubSetting)
        {
            try
            {
                db.Entry(ChatHubSetting).State = EntityState.Modified;
                db.SaveChanges();
                return ChatHubSetting;
            }
            catch
            {
                throw;
            }
        }

        #endregion

    }
}
 