using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using Oqtane.Services;
using System.Linq;
using System.Timers;
using Oqtane.Shared.Models;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Modules;
using System.Threading;
using System.Data;
using Microsoft.AspNetCore.SignalR;
using BlazorAlerts;
using System.Net;
using BlazorDraggableList;
using Oqtane.Shared.Enums;
using Oqtane.Shared.Extensions;
using BlazorVideo;
using BlazorBrowserResize;

namespace Oqtane.ChatHubs.Services
{

    public class ChatHubService : ServiceBase, IChatHubService, IService, IDisposable
    {

        public HttpClient HttpClient { get; set; }
        public NavigationManager NavigationManager { get; set; }
        public SiteState SiteState { get; set; }
        public IJSRuntime JSRuntime { get; set; }
        public VideoService VideoService { get; set; }
        public ScrollService ScrollService { get; set; }
        public BlazorAlertsService BlazorAlertsService { get; set; }
        public BlazorDraggableListService BlazorDraggableListService { get; set; }
        public BlazorBrowserResizeService BrowserResizeService { get; set; }
        public BlazorVideoService BlazorVideoService { get; set; }

        public HubConnection Connection { get; set; }
        public ChatHubUser ConnectedUser { get; set; }
        
        public Cookie IdentityCookie { get; set; }
        public string ContextRoomId { get; set; }
        public int ModuleId { get; set; }

        public List<ChatHubRoom> Lobbies { get; set; } = new List<ChatHubRoom>();
        public List<ChatHubRoom> Rooms { get; set; } = new List<ChatHubRoom>();
        public List<ChatHubInvitation> Invitations { get; set; } = new List<ChatHubInvitation>();
        public List<ChatHubUser> IgnoredUsers { get; set; } = new List<ChatHubUser>();
        public List<ChatHubUser> IgnoredByUsers { get; set; } = new List<ChatHubUser>();
        public Dictionary<int, dynamic> LocalStreamTasks { get; set; } = new Dictionary<int, dynamic>();
        public List<int> RemoteStreamTasks { get; set; } = new List<int>();

        public System.Timers.Timer GetLobbyRoomsTimer { get; set; } = new System.Timers.Timer();

        public event EventHandler OnUpdateUI;
        public event EventHandler<ChatHubUser> OnUpdateConnectedUserEvent;
        public event EventHandler<ChatHubRoom> OnAddChatHubRoomEvent;
        public event EventHandler<ChatHubRoom> OnRemoveChatHubRoomEvent;
        public event EventHandler<dynamic> OnAddChatHubUserEvent;
        public event EventHandler<dynamic> OnRemoveChatHubUserEvent;
        public event EventHandler<ChatHubMessage> OnAddChatHubMessageEvent;
        public event EventHandler<ChatHubInvitation> OnAddChatHubInvitationEvent;
        public event EventHandler<ChatHubInvitation> OnRemoveChatHubInvitationEvent;
        public event EventHandler<ChatHubWaitingRoomItem> OnAddChatHubWaitingRoomItemEvent;
        public event EventHandler<ChatHubWaitingRoomItem> OnRemovedChatHubWaitingRoomItemEvent;
        public event EventHandler<ChatHubUser> OnAddIgnoredUserEvent;
        public event EventHandler<ChatHubUser> OnRemoveIgnoredUserEvent;
        public event EventHandler<ChatHubUser> OnAddIgnoredByUserEvent;
        public event EventHandler<ChatHubUser> OnRemoveIgnoredByUserEvent;
        public event EventHandler<dynamic> OnAddModeratorEvent;
        public event EventHandler<dynamic> OnRemoveModeratorEvent;
        public event EventHandler<dynamic> OnAddWhitelistUserEvent;
        public event EventHandler<dynamic> OnRemoveWhitelistUserEvent;
        public event EventHandler<dynamic> OnAddBlacklistUserEvent;
        public event EventHandler<dynamic> OnRemoveBlacklistUserEvent;
        public event EventHandler<dynamic> OnDownloadBytes;
        public event EventHandler<int> OnClearHistoryEvent;
        public event EventHandler<ChatHubUser> OnDisconnectEvent;
        public event EventHandler<dynamic> OnExceptionEvent;

        public ChatHubService(HttpClient httpClient, SiteState siteState, NavigationManager navigationManager, IJSRuntime JSRuntime, VideoService videoService, ScrollService scrollService, BlazorAlertsService blazorAlertsService, BlazorDraggableListService blazorDraggableListService, BlazorBrowserResizeService browserResizeService, BlazorVideoService blazorVideoService ) : base (httpClient)
        {
            this.HttpClient = httpClient;
            this.SiteState = siteState;
            this.NavigationManager = navigationManager;
            this.JSRuntime = JSRuntime;
            this.VideoService = videoService;
            this.ScrollService = scrollService;
            this.BlazorAlertsService = blazorAlertsService;
            this.BlazorDraggableListService = blazorDraggableListService;
            this.BrowserResizeService = browserResizeService;
            this.BlazorVideoService = blazorVideoService;

            this.VideoService.VideoServiceExtension.OnDataAvailableEventHandler += async (object sender, dynamic e) => await OnDataAvailableEventHandlerExecute(e.dataURI, e.roomId, e.dataType);
            this.VideoService.VideoServiceExtension.OnPauseLivestreamTask += (object sender, int e) => OnPauseLivestreamTaskExecute(sender, e);
            this.VideoService.OnContinueLivestreamTask += (object sender, int e) => OnContinueLivestreamTaskExecute(sender, e);

            this.BlazorAlertsService.OnAlertConfirmed += OnAlertConfirmedExecute;

            this.OnUpdateConnectedUserEvent += OnUpdateConnectedUserExecute;
            this.OnAddChatHubRoomEvent += OnAddChatHubRoomExecute;
            this.OnRemoveChatHubRoomEvent += OnRemoveChatHubRoomExecute;
            this.OnAddChatHubUserEvent += OnAddChatHubUserExecute;
            this.OnRemoveChatHubUserEvent += OnRemoveChatHubUserExecute;
            this.OnAddChatHubMessageEvent += OnAddChatHubMessageExecute;
            this.OnAddChatHubInvitationEvent += OnAddChatHubInvitationExecute;
            this.OnRemoveChatHubInvitationEvent += OnRemoveChatHubInvitationExecute;
            this.OnAddChatHubWaitingRoomItemEvent += OnAddChatHubWaitingRoomItemExecute;
            this.OnRemovedChatHubWaitingRoomItemEvent += OnRemovedChathubWaitingRoomItemExecute;
            this.OnAddIgnoredUserEvent += OnAddIngoredUserExexute;
            this.OnRemoveIgnoredUserEvent += OnRemoveIgnoredUserExecute;
            this.OnAddIgnoredByUserEvent += OnAddIgnoredByUserExecute;
            this.OnAddModeratorEvent += OnAddModeratorExecute;
            this.OnRemoveModeratorEvent += OnRemoveModeratorExecute;
            this.OnAddWhitelistUserEvent += OnAddWhitelistUserExecute;
            this.OnRemoveWhitelistUserEvent += OnRemoveWhitelistUserExecute;
            this.OnAddBlacklistUserEvent += OnAddBlacklistUserExecute;
            this.OnRemoveBlacklistUserEvent += OnRemoveBlacklistUserExecute;
            this.OnDownloadBytes += OnDownloadBytesExecuteAsync;
            this.OnRemoveIgnoredByUserEvent += OnRemoveIgnoredByUserExecute;
            this.OnClearHistoryEvent += OnClearHistoryExecute;
            this.OnDisconnectEvent += OnDisconnectExecute;

            GetLobbyRoomsTimer.Elapsed += new ElapsedEventHandler(OnGetLobbyRoomsTimerElapsed);
            GetLobbyRoomsTimer.Interval = 10000;
            GetLobbyRoomsTimer.Enabled = true;
        }

        public void BuildGuestConnection(string username, int moduleId)
        {
            StringBuilder urlBuilder = new StringBuilder();
            var chatHubConnection = this.NavigationManager.BaseUri + "chathub";

            urlBuilder.Append(chatHubConnection);
            urlBuilder.Append("?guestname=" + username);

            var url = urlBuilder.ToString();
            Connection = new HubConnectionBuilder().WithUrl(url, options =>
            {
                options.Cookies.Add(this.IdentityCookie);
                options.Headers["moduleid"] = moduleId.ToString();
                options.Headers["platform"] = "Oqtane";
                options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
            })
            .AddMessagePackProtocol()
            .AddNewtonsoftJsonProtocol(options => {
                options.PayloadSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            })
            .Build();
        }
        public void RegisterHubConnectionHandlers()
        {
            Connection.Reconnecting += (ex) =>
            {
                if (ex != null)
                {
                    this.HandleException(new Exception(ex.Message, ex));
                }

                return Task.CompletedTask;
            };
            Connection.Reconnected += (msg) =>
            {
                if (msg != null)
                {
                    this.HandleException(new Exception(msg));
                }

                return Task.CompletedTask;
            };
            Connection.Closed += (ex) =>
            {
                if (ex != null)
                {
                    this.HandleException(new Exception(ex.Message, ex));
                }

                this.Rooms.Clear();
                this.RunUpdateUI();
                return Task.CompletedTask;
            };

            this.Connection.On("OnUpdateConnectedUser", (ChatHubUser user) => OnUpdateConnectedUserEvent(this, user));
            this.Connection.On("AddRoom", (ChatHubRoom room) => OnAddChatHubRoomEvent(this, room));
            this.Connection.On("RemoveRoom", (ChatHubRoom room) => OnRemoveChatHubRoomEvent(this, room));
            this.Connection.On("AddUser", (ChatHubUser user, string roomId) => OnAddChatHubUserEvent(this, new { userModel = user, roomId = roomId }));
            this.Connection.On("RemoveUser", (ChatHubUser user, string roomId) => OnRemoveChatHubUserEvent(this, new { userModel = user, roomId = roomId }));
            this.Connection.On("AddMessage", (ChatHubMessage message) => OnAddChatHubMessageEvent(this, message));
            this.Connection.On("AddInvitation", (ChatHubInvitation invitation) => OnAddChatHubInvitationEvent(this, invitation));
            this.Connection.On("RemoveInvitation", (ChatHubInvitation invitation) => OnRemoveChatHubInvitationEvent(this, invitation));
            this.Connection.On("AddWaitingRoomItem", (ChatHubWaitingRoomItem waitingRoomItem) => OnAddChatHubWaitingRoomItemEvent(this, waitingRoomItem));
            this.Connection.On("RemovedWaitingRoomItem", (ChatHubWaitingRoomItem waitingRoomItem) => OnRemovedChatHubWaitingRoomItemEvent(this, waitingRoomItem));
            this.Connection.On("AddIgnoredUser", (ChatHubUser ignoredUser) => OnAddIgnoredUserEvent(this, ignoredUser));
            this.Connection.On("RemoveIgnoredUser", (ChatHubUser ignoredUser) => OnRemoveIgnoredUserEvent(this, ignoredUser));
            this.Connection.On("AddIgnoredByUser", (ChatHubUser ignoredUser) => OnAddIgnoredByUserEvent(this, ignoredUser));
            this.Connection.On("RemoveIgnoredByUser", (ChatHubUser ignoredUser) => OnRemoveIgnoredByUserEvent(this, ignoredUser));
            this.Connection.On("DownloadBytes", (string dataURI, int roomId, string dataType) => OnDownloadBytes(this, new { dataURI = dataURI, roomId = roomId, dataType = dataType }));
            this.Connection.On("AddModerator", (ChatHubModerator moderator, int roomId) => OnAddModeratorEvent(this, new { moderator = moderator, roomId = roomId }));
            this.Connection.On("RemoveModerator", (ChatHubModerator moderator, int roomId) => OnRemoveModeratorEvent(this, new { moderator = moderator, roomId = roomId }));
            this.Connection.On("AddWhitelistUser", (ChatHubWhitelistUser whitelistUser, int roomId) => OnAddWhitelistUserEvent(this, new { whitelistUser = whitelistUser, roomId = roomId }));
            this.Connection.On("RemoveWhitelistUser", (ChatHubWhitelistUser whitelistUser, int roomId) => OnRemoveWhitelistUserEvent(this, new { whitelistUser = whitelistUser, roomId = roomId }));
            this.Connection.On("AddBlacklistUser", (ChatHubBlacklistUser blacklistUser, int roomId) => OnAddBlacklistUserEvent(this, new { blacklistUser = blacklistUser, roomId = roomId }));
            this.Connection.On("RemoveBlacklistUser", (ChatHubBlacklistUser blacklistUser, int roomId) => OnRemoveBlacklistUserEvent(this, new { blacklistUser = blacklistUser, roomId = roomId }));
            this.Connection.On("ClearHistory", (int roomId) => OnClearHistoryEvent(this, roomId));
            this.Connection.On("Disconnect", (ChatHubUser user) => OnDisconnectEvent(this, user));
        }
        public async Task ConnectAsync()
        {
            await this.Connection.StartAsync().ContinueWith(async task =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);

                    await this.Connection.SendAsync("Init").ContinueWith((task) =>
                    {
                        if (task.IsCompleted)
                        {
                            this.HandleException(task);
                        }
                    });
                }
            });
        }

        public async Task StartVideoChat(int roomId)
        {

            this.BlazorVideoService.StartBroadcasting(roomId.ToString());

            try
            {

                /*
                var room = this.Rooms.FirstOrDefault(item => item.Id == roomId);
                await this.StopVideoChat(room.Id);

                if (room.CreatorId == this.ConnectedUser.UserId)
                {
                    await this.VideoService.StartBroadcasting(room.Id);

                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    CancellationToken token = tokenSource.Token;
                    Task task = new Task(async () => await this.StreamTaskImplementation(room.Id, token), token);
                    this.AddLocalStreamTask(room.Id, task, tokenSource);
                    task.Start();
                }
                else
                {
                    this.AddRemoteStreamTask(roomId);
                    await this.VideoService.StartStreaming(room.Id);
                }
                */
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }
        public async Task StopVideoChat(int roomId)
        {
            var room = this.Rooms.FirstOrDefault(item => item.Id == roomId);
            if (room.CreatorId == this.ConnectedUser.UserId)
            {
                this.RemoveLocalStreamTask(roomId);
                await this.VideoService.CloseLivestream(roomId);
            }
            else
            {
                this.RemoveRemoteStreamTask(roomId);
                await this.VideoService.CloseLivestream(roomId);
            }
        }
        public async Task RestartStreamTaskIfExists(int roomId)
        {
            if (this.LocalStreamTasks.Any(item => item.Key == roomId) || this.RemoteStreamTasks.Any(item => item == roomId))
            {
                await this.StartVideoChat(roomId);
            }

            this.RunUpdateUI();
        }
        public void AddLocalStreamTask(int roomId, Task task, CancellationTokenSource tokenSource)
        {
            this.RemoveLocalStreamTask(roomId);

            dynamic obj = new { task = task, tokenSource = tokenSource };
            this.LocalStreamTasks.Add(roomId, obj);
            this.RunUpdateUI();
        }
        public void RemoveLocalStreamTask(int roomId)
        {
            List<KeyValuePair<int, dynamic>> list = this.LocalStreamTasks.Where(item => item.Key == roomId).ToList();
            if (list.Any())
            {
                KeyValuePair<int, dynamic> keyValuePair = list.FirstOrDefault();
                dynamic obj = keyValuePair.Value;

                obj.tokenSource?.Cancel();
                obj.task?.Dispose();

                this.LocalStreamTasks.Remove(keyValuePair.Key);
            }
        }
        public void AddRemoteStreamTask(int roomId)
        {
            var items = this.RemoteStreamTasks.Where(id => id == roomId);
            if(!items.Any())
            {
                this.RemoteStreamTasks.Add(roomId);
                this.RunUpdateUI();
            }
        }
        public void RemoveRemoteStreamTask(int roomId)
        {
            var items = this.RemoteStreamTasks.Where(id => id == roomId);
            if (items.Any())
            {
                this.RemoteStreamTasks.Remove(roomId);
            }
        }
        public async Task StreamTaskImplementation(int roomId, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await this.VideoService.StopSequence(roomId);
                    await this.VideoService.StartSequence(roomId);

                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        public async Task DisposeStreamTasksAsync()
        {
            foreach(var task in LocalStreamTasks)
            {
                await this.StopVideoChat(task.Key);
            }
        }
        public async Task OnDataAvailableEventHandlerExecute(string dataURI, int roomId, string dataType)
        {
            try
            {
                if(this.Connection?.State == HubConnectionState.Connected)
                {
                    int maxLength = 4200;
                    async IAsyncEnumerable<string> broadcastData()
                    {
                        for (var i = 0; i < dataURI.Length; i += maxLength)
                        {
                            yield return dataURI.Substring(i, Math.Min(maxLength, dataURI.Length - i));
                        }
                    }

                    await this.Connection.SendAsync("UploadBytes", broadcastData(), roomId, dataType).ContinueWith((task) =>
                    {
                        if (task.IsCompleted)
                        {
                            this.HandleException(task);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void OnPauseLivestreamTaskExecute(object sender, int roomId)
        {
            List<KeyValuePair<int, dynamic>> list = this.LocalStreamTasks.Where(item => item.Key == roomId).ToList();
            if (list.Any())
            {
                KeyValuePair<int, dynamic> keyValuePair = list.FirstOrDefault();
                dynamic obj = keyValuePair.Value;
                obj.tokenSource.Cancel();
                obj.task.Dispose();
            }
        }
        public async Task OnContinueLivestreamTaskExecute(object sender, int roomId)
        {
            List<KeyValuePair<int, dynamic>> localList = this.LocalStreamTasks.Where(item => item.Key == roomId).ToList();
            List<int> remoteList = this.RemoteStreamTasks.Where(item => item == roomId).ToList();

            if (localList.Any() || remoteList.Any())
            {
                await this.StartVideoChat(roomId);
            }
        }
        public async void OnDownloadBytesExecuteAsync(object sender, dynamic e)
        {
            string dataURI = e.dataURI;
            int roomId = e.roomId;
            string dataType = e.dataType;

            try
            {
                await this.VideoService.AppendBuffer(dataURI, roomId, dataType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task EnterChatRoom(int roomId)
        {
            await this.Connection.InvokeAsync("EnterChatRoom", roomId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public async Task LeaveChatRoom(int roomId)
        {
            await this.Connection.InvokeAsync("LeaveChatRoom", roomId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public async Task GetLobbyRooms(int moduleId)
        {
            try
            {
                this.Lobbies = await this.GetChatHubRoomsByModuleIdAsync(moduleId);
                this.SortLobbyRooms();
                this.RunUpdateUI();
            }
            catch (Exception ex)
            {
                // !!!Important | This Try Catch Block Is Necessary
                this.HandleException(ex);
            }
        }
        public async Task GetIgnoredUsers()
        {
            await this.Connection.InvokeAsync<List<ChatHubUser>>("GetIgnoredUsers").ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);

                    var ignoredUsers = task.Result;
                    if (ignoredUsers != null)
                    {
                        foreach (var user in ignoredUsers)
                        {
                            this.IgnoredUsers.AddIgnoredUser(user);
                        }
                    }
                }
            });
        }
        public async Task GetIgnoredByUsers()
        {
            await this.Connection.InvokeAsync<List<ChatHubUser>>("GetIgnoredByUsers").ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);

                    var ignoredByUsers = task.Result;
                    if (ignoredByUsers != null)
                    {
                        foreach (var user in ignoredByUsers)
                        {
                            this.IgnoredByUsers.AddIgnoredByUser(user);
                        }
                    }
                }
            });
        }

        public void SortLobbyRooms()
        {
            if (this.Lobbies != null && this.Lobbies.Any())
            {
                this.Lobbies = this.Lobbies.OrderByDescending(item => item.Users?.Count()).ThenByDescending(item => item.CreatedOn).OrderBy(item => (int)Enum.Parse(typeof(ChatHubRoomStatus), item.Status)).Take(1000).ToList();
            }
        }

        public async Task SendMessage(string content, int roomId, int moduleId)
        {
            await this.Connection.InvokeAsync("SendMessage", content, roomId, moduleId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public void IgnoreUser_Clicked(int userId, int roomId, string username)
        {
            this.Connection.InvokeAsync("IgnoreUser", username).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public void UnignoreUser_Clicked(string username)
        {
            this.Connection.InvokeAsync("UnignoreUser", username).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public void AddModerator_Clicked(int userId, int roomId)
        {
            this.Connection.InvokeAsync("AddModerator", userId, roomId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public void RemoveModerator_Clicked(int userId, int roomId)
        {
            this.Connection.InvokeAsync("RemoveModerator", userId, roomId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public void AddWhitelistUser_Clicked(int userId, int roomId)
        {
            this.Connection.InvokeAsync("AddWhitelistUser", userId, roomId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public void RemoveWhitelistUser_Clicked(int userId, int roomId)
        {
            this.Connection.InvokeAsync("RemoveWhitelistUser", userId, roomId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public void AddBlacklistUser_Clicked(int userId, int roomId)
        {
            this.Connection.InvokeAsync("AddBlacklistUser", userId, roomId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public void RemoveBlacklistUser_Clicked(int userId, int roomId)
        {
            this.Connection.InvokeAsync("RemoveBlacklistUser", userId, roomId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }
        public void RemoveWaitingRoomItem_Clicked(ChatHubWaitingRoomItem waitingRoomItem)
        {
            this.AddWhitelistUser_Clicked(waitingRoomItem.UserId, waitingRoomItem.RoomId);

            this.Connection.InvokeAsync("RemoveWaitingRoomItem", waitingRoomItem).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });

            this.Rooms.FirstOrDefault(item => item.Id == waitingRoomItem.RoomId).WaitingRoomItems.Remove(waitingRoomItem);
            this.RunUpdateUI();
        }
        public async Task DisconnectAsync()
        {
            if (Connection.State != HubConnectionState.Disconnected)
            {
                await Connection.StopAsync();
            }
        }

        private void OnAddChatHubRoomExecute(object sender, ChatHubRoom room)
        {
            this.Rooms.AddRoom(room);
            this.RunUpdateUI();
        }
        private void OnRemoveChatHubRoomExecute(object sender, ChatHubRoom room)
        {
            this.Rooms.RemoveRoom(room);
            this.RunUpdateUI();
        }
        private void OnAddChatHubUserExecute(object sender, dynamic obj)
        {
            this.Rooms.AddUser(obj.userModel as ChatHubUser, obj.roomId as string);
            this.RunUpdateUI();
        }
        private void OnRemoveChatHubUserExecute(object sender, dynamic obj)
        {
            this.Rooms.RemoveUser(obj.userModel as ChatHubUser, obj.roomId as string);
            this.RunUpdateUI();
        }
        public async void OnAddChatHubMessageExecute(object sender, ChatHubMessage message)
        {
            ChatHubRoom room = this.Rooms.FirstOrDefault(item => item.Id == message.ChatHubRoomId);
            room.AddMessage(message);

            if (message.ChatHubRoomId.ToString() != this.ContextRoomId)
            {
                this.Rooms.FirstOrDefault(room => room.Id == message.ChatHubRoomId).UnreadMessages++;
            }

            this.RunUpdateUI();

            string elementId = string.Concat("#message-window-", this.ModuleId.ToString(), "-", message.ChatHubRoomId.ToString());
            await this.ScrollService.ScrollToBottom(elementId);
        }
        private void OnAddChatHubInvitationExecute(object sender, ChatHubInvitation item)
        {
            this.Invitations.AddInvitation(item);
        }
        private void OnRemoveChatHubInvitationExecute(object sender, ChatHubInvitation item)
        {
            this.Invitations.RemoveInvitation(item.Guid);
        }
        private void OnAddChatHubWaitingRoomItemExecute(object sender, ChatHubWaitingRoomItem e)
        {
            this.Rooms.FirstOrDefault(item => item.Id == e.RoomId).WaitingRoomItems.Add(e);
        }
        private async void OnRemovedChathubWaitingRoomItemExecute(object sender, ChatHubWaitingRoomItem e)
        {
            var room = await this.GetChatHubRoomAsync(e.RoomId, this.ModuleId);
            this.BlazorAlertsService.NewBlazorAlert($"You have been granted access to the room named {room.Title}. Do you like to enter??", "Javascript Application", PositionType.Fixed, true, e.RoomId.ToString());
        }
        private void OnAddIngoredUserExexute(object sender, ChatHubUser user)
        {
            this.IgnoredUsers.AddIgnoredUser(user);
            this.RunUpdateUI();
        }
        private void OnRemoveIgnoredUserExecute(object sender, ChatHubUser user)
        {
            this.IgnoredUsers.RemoveIgnoredUser(user);
            this.RunUpdateUI();
        }
        private void OnAddIgnoredByUserExecute(object sender, ChatHubUser user)
        {
            this.IgnoredByUsers.AddIgnoredByUser(user);
            this.RunUpdateUI();
        }
        private void OnRemoveIgnoredByUserExecute(object sender, ChatHubUser user)
        {
            this.IgnoredByUsers.RemoveIgnoredByUser(user);
            this.RunUpdateUI();
        }
        private void OnAddModeratorExecute(object sender, dynamic e)
        {
            this.Rooms.AddModerator(e.moderator as ChatHubModerator, (int)e.roomId);
            this.RunUpdateUI();
        }
        private void OnRemoveModeratorExecute(object sender, dynamic e)
        {
            this.Rooms.RemoveModerator(e.moderator as ChatHubModerator, (int)e.roomId);
            this.RunUpdateUI();
        }
        private void OnAddWhitelistUserExecute(object sender, dynamic e)
        {
            this.Rooms.AddWhitelistUser(e.whitelistUser as ChatHubWhitelistUser, (int)e.roomId);
            this.RunUpdateUI();
        }
        private void OnRemoveWhitelistUserExecute(object sender, dynamic e)
        {
            this.Rooms.RemoveWhitelistUser(e.whitelistUser as ChatHubWhitelistUser, (int)e.roomId);
            this.RunUpdateUI();
        }
        private void OnAddBlacklistUserExecute(object sender, dynamic e)
        {
            this.Rooms.AddBlacklistUser(e.blacklistUser as ChatHubBlacklistUser, (int)e.roomId);
            this.RunUpdateUI();
        }
        private void OnRemoveBlacklistUserExecute(object sender, dynamic e)
        {
            this.Rooms.RemoveBlacklistUser(e.blacklistUser as ChatHubBlacklistUser, (int)e.roomId);
            this.RunUpdateUI();
        }
        private void OnClearHistoryExecute(object sender, int roomId)
        {
            this.ClearHistory(roomId);
        }
        public void OnUpdateConnectedUserExecute(object sender, ChatHubUser user)
        {
            this.ConnectedUser = user;
            this.RunUpdateUI();
        }
        private async void OnDisconnectExecute(object sender, ChatHubUser user)
        {
            await this.DisconnectAsync();
        }

        private async void OnGetLobbyRoomsTimerElapsed(object source, ElapsedEventArgs e)
        {
            await this.GetLobbyRooms(this.ModuleId);
        }

        public async void OnAlertConfirmedExecute(object sender, dynamic obj)
        {
            bool confirmed = (bool)obj.confirmed;
            BlazorAlertsModel model = (BlazorAlertsModel)obj.model;

            if (confirmed)
            {
                await this.EnterChatRoom(Convert.ToInt32(model.Id));
            }
        }

        public void ClearHistory(int roomId)
        {
            var room = this.Rooms.FirstOrDefault(x => x.Id == roomId);
            room.Messages.Clear();
            this.RunUpdateUI();
        }
        public void ToggleUserlist(ChatHubRoom room)
        {
            room.ShowUserlist = !room.ShowUserlist;
        }
        public string AutocompleteUsername(string msgInput, int roomId, int autocompleteCounter, string pressedKey)
        {
            List<string> words = msgInput.Trim().Split(' ').ToList();
            string lastWord = words.Last();

            var room = this.Rooms.FirstOrDefault(item => item.Id == roomId);
            var users = room.Users.Where(x => x.DisplayName.StartsWith(lastWord));

            if (users.Any())
            {
                autocompleteCounter = autocompleteCounter % users.Count();

                words.Remove(lastWord);
                if (pressedKey == "Enter")
                    words.Add(users.ToArray()[autocompleteCounter].DisplayName);

                msgInput = string.Join(' ', words);
            }

            return msgInput;
        }

        public void HandleException(Task task)
        {
            if (task.Exception != null)
            {
                this.HandleException(task.Exception);
            }
        }
        public void HandleException(Exception exception)
        {
            string message = string.Empty;
            if (exception.InnerException != null && exception.InnerException is HubException)
            {
                int startIndex = exception.Message.IndexOf("HubException:");
                int endIndex = exception.Message.Length - startIndex - 1;

                message = exception.Message.Substring(startIndex, endIndex);
            }
            else
            {
                message = exception.ToString();
            }

            BlazorAlertsService.NewBlazorAlert(message, "Javascript Application", PositionType.Fixed);
            this.RunUpdateUI();
        }

        public void Dispose()
        {
            this.VideoService.VideoServiceExtension.OnDataAvailableEventHandler -= async (object sender, dynamic e) => await OnDataAvailableEventHandlerExecute(e.dataURI, e.roomId, e.dataType);
            this.VideoService.VideoServiceExtension.OnPauseLivestreamTask -= (object sender, int e) => OnPauseLivestreamTaskExecute(sender, e);
            this.VideoService.OnContinueLivestreamTask -= (object sender, int e) => OnContinueLivestreamTaskExecute(sender, e);
            this.DisposeStreamTasksAsync();

            this.BlazorAlertsService.OnAlertConfirmed -= OnAlertConfirmedExecute;

            this.OnUpdateConnectedUserEvent -= OnUpdateConnectedUserExecute;
            this.OnAddChatHubRoomEvent -= OnAddChatHubRoomExecute;
            this.OnRemoveChatHubRoomEvent -= OnRemoveChatHubRoomExecute;
            this.OnAddChatHubUserEvent -= OnAddChatHubUserExecute;
            this.OnRemoveChatHubUserEvent -= OnRemoveChatHubUserExecute;
            this.OnAddChatHubMessageEvent -= OnAddChatHubMessageExecute;
            this.OnAddChatHubInvitationEvent -= OnAddChatHubInvitationExecute;
            this.OnRemoveChatHubInvitationEvent -= OnRemoveChatHubInvitationExecute;
            this.OnAddChatHubWaitingRoomItemEvent -= OnAddChatHubWaitingRoomItemExecute;
            this.OnRemovedChatHubWaitingRoomItemEvent -= OnRemovedChathubWaitingRoomItemExecute;
            this.OnAddIgnoredUserEvent -= OnAddIngoredUserExexute;
            this.OnRemoveIgnoredUserEvent -= OnRemoveIgnoredUserExecute;
            this.OnAddIgnoredByUserEvent -= OnAddIgnoredByUserExecute;
            this.OnAddModeratorEvent -= OnAddModeratorExecute;
            this.OnRemoveModeratorEvent -= OnRemoveModeratorExecute;
            this.OnDownloadBytes -= OnDownloadBytesExecuteAsync;
            this.OnRemoveIgnoredByUserEvent -= OnRemoveIgnoredByUserExecute;
            this.OnClearHistoryEvent -= OnClearHistoryExecute;
            this.OnDisconnectEvent -= OnDisconnectExecute;

            GetLobbyRoomsTimer.Elapsed -= new ElapsedEventHandler(OnGetLobbyRoomsTimerElapsed);

            this.Connection.StopAsync();
        }

        public void RunUpdateUI()
        {
            this.OnUpdateUI.Invoke(this, EventArgs.Empty);
        }

        public string apiurl
        {
            //get { return NavigationManager.BaseUri + "api/ChatHub"; }
            get { return CreateApiUrl(SiteState.Alias, "ChatHub"); }
        }
        public async Task<List<ChatHubRoom>> GetChatHubRoomsByModuleIdAsync(int ModuleId)
        {
            return await HttpClient.GetJsonAsync<List<ChatHubRoom>>(apiurl + "/getchathubroomsbymoduleid?entityid=" + ModuleId);
        }
        public async Task<ChatHubRoom> GetChatHubRoomAsync(int ChatHubRoomId, int ModuleId)
        {
            return await HttpClient.GetJsonAsync<ChatHubRoom>(apiurl + "/getchathubroom/" + ChatHubRoomId + "?entityid=" + ModuleId);
        }
        public async Task<ChatHubRoom> AddChatHubRoomAsync(ChatHubRoom ChatHubRoom)
        {
            return await HttpClient.PostJsonAsync<ChatHubRoom>(apiurl + "/addchathubroom" + "?entityid=" + ChatHubRoom.ModuleId, ChatHubRoom);
        }
        public async Task UpdateChatHubRoomAsync(ChatHubRoom ChatHubRoom)
        {
            await HttpClient.PutJsonAsync(apiurl + "/updatechathubroom/" + ChatHubRoom.Id + "?entityid=" + ChatHubRoom.ModuleId, ChatHubRoom);
        }
        public async Task DeleteChatHubRoomAsync(int ChatHubRoomId, int ModuleId)
        {
            await HttpClient.DeleteAsync(apiurl + "/deletechathubroom/" + ChatHubRoomId + "?entityid=" + ModuleId);
        }
        public async Task DeleteRoomImageAsync(int ChatHubRoomId, int ModuleId)
        {
            await HttpClient.DeleteAsync(apiurl + "/deleteroomimage/" + ChatHubRoomId + "?entityid=" + ModuleId);
        }
        public async Task FixCorruptConnections(int ModuleId)
        {
            await HttpClient.DeleteAsync(apiurl + "/fixcorruptconnections" + "?entityid=" + ModuleId);
        }
        
    }
}
