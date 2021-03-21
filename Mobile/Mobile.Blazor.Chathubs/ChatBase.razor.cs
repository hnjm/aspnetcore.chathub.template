using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Oqtane.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mobile.Blazor.Chathubs
{
    public class ChatBase : ComponentBase
    {

        [Inject]
        protected ChatHubService ChatHubService { get; set; }

        protected ExceptionModal ExceptionModalReference { get; set; }

        protected GuestLogin GuestLoginReference { get; set; }

        protected string ExceptionMessage { get; set; } = string.Empty;

        public ChatBase() { }

        protected override void OnInitialized()
        {
            this.ChatHubService.UpdateUI += UpdateUIStateHasChanged;
            this.ChatHubService.OnAddChatHubMessageEvent += OnAddChatHubMessageExecute;
            this.ChatHubService.OnExceptionEvent += OnExceptionExecute;
        }

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                await this.ChatHubService.GetLobbyRooms();   
            }
            catch (Exception ex)
            {
                this.ChatHubService.HandleException(ex);
            }

            await base.OnParametersSetAsync();
        }

        public async Task ConnectAsGuest()
        {
            try
            {
                if (this.ChatHubService.Connection?.State == HubConnectionState.Connected
                 || this.ChatHubService.Connection?.State == HubConnectionState.Connecting
                 || this.ChatHubService.Connection?.State == HubConnectionState.Reconnecting)
                {
                    this.MobileBlazorAlert("Your already connected.");
                }

                this.ChatHubService.BuildGuestConnection(this.GuestLoginReference.Guestname);
                this.ChatHubService.RegisterHubConnectionHandlers();
                await this.ChatHubService.ConnectAsync();
            }
            catch (Exception ex)
            {
                this.ChatHubService.HandleException(ex);
            }
        }

        private async void OnAddChatHubMessageExecute(object sender, ChatHubMessage message)
        {
            if (message.ChatHubRoomId.ToString() != ChatHubService.ContextRoomId)
            {
                ChatHubService.Rooms.FirstOrDefault(room => room.Id == message.ChatHubRoomId).UnreadMessages++;
                this.UpdateUIStateHasChanged();
            }
        }

        private void UpdateUIStateHasChanged()
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public async void OnExceptionExecute(object sender, dynamic dynamicObject)
        {
            Exception exception = dynamicObject.Exception;
            ChatHubUser contextUser = dynamicObject.ConnectedUser;

            var msg = string.Empty;
            if (exception.InnerException != null && exception.InnerException is HubException)
            {
                msg = exception.InnerException.Message.Substring(exception.InnerException.Message.IndexOf("HubException"));
            }
            else
            {
                msg = exception.Message;
            }

            this.MobileBlazorAlert(msg);
        }

        public async void MobileBlazorAlert(string msg = null)
        {
            await InvokeAsync(() =>
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    this.ExceptionMessage = msg;
                }

                this.ExceptionModalReference.OnExceptionModalOpened();
                StateHasChanged();
            });
        }
    }
}
