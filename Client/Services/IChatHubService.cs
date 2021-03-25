using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Oqtane.Shared;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Services
{
    public interface IChatHubService
    {

        HttpClient HttpClient { get; set; }
        NavigationManager NavigationManager { get; set; }
        SiteState SiteState { get; set; }
        IJSRuntime JSRuntime { get; set; }

        HubConnection Connection { get; set; }
        ChatHubUser ConnectedUser { get; set; }

        Cookie IdentityCookie { get; set; }
        string ContextRoomId { get; set; }
        int ModuleId { get; set; }

        List<ChatHubRoom> Lobbies { get; set; }
        List<ChatHubRoom> Rooms { get; set; }

        List<ChatHubInvitation> Invitations { get; set; }

        List<ChatHubUser> IgnoredUsers { get; set; }
        List<ChatHubUser> IgnoredByUsers { get; set; }

        System.Timers.Timer GetLobbyRoomsTimer { get; set; }

        public event EventHandler OnUpdateUI;
        public event EventHandler<ChatHubUser> OnUpdateConnectedUserEvent;
        public event EventHandler<ChatHubRoom> OnAddChatHubRoomEvent;
        public event EventHandler<ChatHubRoom> OnRemoveChatHubRoomEvent;
        public event EventHandler<dynamic> OnAddChatHubUserEvent;
        public event EventHandler<dynamic> OnRemoveChatHubUserEvent;
        public event EventHandler<ChatHubMessage> OnAddChatHubMessageEvent;
        public event EventHandler<ChatHubInvitation> OnAddChatHubInvitationEvent;
        public event EventHandler<ChatHubInvitation> OnRemoveChatHubInvitationEvent;
        public event EventHandler<ChatHubUser> OnAddIgnoredUserEvent;
        public event EventHandler<ChatHubUser> OnRemoveIgnoredUserEvent;
        public event EventHandler<ChatHubUser> OnAddIgnoredByUserEvent;
        public event EventHandler<ChatHubUser> OnRemoveIgnoredByUserEvent;
        public event EventHandler<int> OnClearHistoryEvent;
        public event EventHandler<ChatHubUser> OnDisconnectEvent;
        public event EventHandler<dynamic> OnExceptionEvent;

        string apiurl { get; }

        void BuildGuestConnection(string username, int moduleId);

        void RegisterHubConnectionHandlers();

        Task ConnectAsync();

        Task EnterChatRoom(int roomId);

        Task LeaveChatRoom(int roomId);

        Task GetLobbyRooms(int moduleId);

        Task SendMessage(string content, int roomId, int moduleId);

        Task<List<ChatHubRoom>> GetChatHubRoomsByModuleIdAsync(int ModuleId);
        Task<ChatHubRoom> GetChatHubRoomAsync(int ChatHubRoomId, int ModuleId);
        Task<ChatHubRoom> AddChatHubRoomAsync(ChatHubRoom ChatHubRoom);
        Task UpdateChatHubRoomAsync(ChatHubRoom ChatHubRoom);
        Task DeleteChatHubRoomAsync(int ChatHubRoomId, int ModuleId);

        void IgnoreUser_Clicked(int userId, int roomId, string username);
        void UnignoreUser_Clicked(string username);

        void AddModerator_Clicked(int userId, int roomId);
        void RemoveModerator_Clicked(int userId, int roomId);

        void AddWhitelistUser_Clicked(int userId, int roomId);
        void RemoveWhitelistUser_Clicked(int userId, int roomId);

        void AddBlacklistUser_Clicked(int userId, int roomId);
        void RemoveBlacklistUser_Clicked(int userId, int roomId);

        void RemoveWaitingRoomItem_Clicked(ChatHubWaitingRoomItem waitingRoomItem);

        void ClearHistory(int roomId);

        void ToggleUserlist(ChatHubRoom room);

        Task FixCorruptConnections(int ModuleId);

        Task DeleteRoomImageAsync(int ChatHubRoomId, int ModuleId);

        string AutocompleteUsername(string msgInput, int roomId, int autocompleteCounter, string pressedKey);

        void HandleException(Exception exception);

        Task DisconnectAsync();

    }
}