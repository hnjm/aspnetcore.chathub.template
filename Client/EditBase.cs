using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.ChatHubs.Services;
using Oqtane.Modules;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Shared.Enums;
using Oqtane.Shared.Models;
using Oqtane.Shared;
using BlazorAlerts;
using System.Collections.Generic;
using BlazorSelect;
using BlazorColorPicker;

namespace Oqtane.ChatHubs
{
    public class EditBase : ModuleBase, IDisposable
    {

        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public SiteState SiteState { get; set; }
        [Inject] public IChatHubService ChatHubService { get; set; }
        [Inject] public BlazorAlertsService BlazorAlertsService { get; set; }
        [Inject] public BlazorColorPickerService BlazorColorPickerService { get; set; }

        protected readonly string FileUploadDropzoneContainerElementId = "EditComponentFileUploadDropzoneContainer";
        protected readonly string FileUploadInputFileElementId = "EditComponentFileUploadInputFileContainer";

        public HashSet<string> SelectionItems { get; set; } = new HashSet<string>();

        public BlazorColorPickerType BlazorColorPickerActiveType { get; set; }

        public HashSet<string> BlazorColorPickerSelectionItems { get; set; } = new HashSet<string>();

        public override SecurityAccessLevel SecurityAccessLevel { get { return SecurityAccessLevel.Anonymous; } }
        public override string Actions { get { return "Add,Edit"; } }

        public int roomId = -1;
        public string title;
        public string content;
        public string backgroundcolor;
        public string type;
        public string imageUrl;
        public string createdby;
        public DateTime createdon;
        public string modifiedby;
        public DateTime modifiedon;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                this.BlazorColorPickerSelectionItems.Add(BlazorColorPickerType.HTML5ColorPicker.ToString());
                this.BlazorColorPickerSelectionItems.Add(BlazorColorPickerType.CustomColorPicker.ToString());
                this.BlazorColorPickerActiveType = BlazorColorPickerType.CustomColorPicker;

                this.SelectionItems.Add(ChatHubRoomType.Public.ToString());
                this.SelectionItems.Add(ChatHubRoomType.Protected.ToString());
                this.SelectionItems.Add(ChatHubRoomType.Private.ToString());

                this.type = ChatHubRoomType.Public.ToString();

                this.BlazorColorPickerService.OnBlazorColorPickerContextColorChangedEvent += OnBlazorColorPickerContextColorChangedExecute;

                this.ChatHubService.OnUpdateUI += (object sender, EventArgs e) => UpdateUIStateHasChanged();
                await this.InitContextRoomAsync();
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Room", MessageType.Error);
            }
        }

        private void OnBlazorColorPickerContextColorChangedExecute(BlazorColorPickerEvent obj)
        {
            this.backgroundcolor = obj.ContextColor;
            this.UpdateUIStateHasChanged();
        }

        public void OnSelect(BlazorSelectEvent e)
        {
            this.type = e.SelectedItem;
            this.UpdateUIStateHasChanged();
        }

        public void BlazorColorPicker_OnSelect(BlazorSelectEvent e)
        {
            this.BlazorColorPickerActiveType = (BlazorColorPickerType)Enum.Parse(typeof(BlazorColorPickerType), e.SelectedItem, true);
            this.UpdateUIStateHasChanged();
        }

        private async Task InitContextRoomAsync()
        {
            try
            {
                if (PageState.QueryString.ContainsKey("roomid"))
                {
                    this.roomId = Int32.Parse(PageState.QueryString["roomid"]);
                    ChatHubRoom room = await this.ChatHubService.GetChatHubRoomAsync(roomId, ModuleState.ModuleId);
                    if (room != null)
                    {
                        this.title = room.Title;
                        this.content = room.Content;
                        this.backgroundcolor = room.BackgroundColor;
                        this.type = room.Type;
                        this.imageUrl = room.ImageUrl;
                        this.createdby = room.CreatedBy;
                        this.createdon = room.CreatedOn;
                        this.modifiedby = room.ModifiedBy;
                        this.modifiedon = room.ModifiedOn;
                    }
                    else
                    {
                        await logger.LogError("Error Loading Room {ChatHubRoomId} {Error}", roomId);
                        ModuleInstance.AddModuleMessage("Error Loading ChatHub", MessageType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Room", MessageType.Error);
            }
        }

        public async Task SaveRoom()
        {
            try
            {                
                if (roomId == -1)
                {
                    ChatHubRoom room = new ChatHubRoom()
                    {
                        ModuleId = ModuleState.ModuleId,
                        Title = this.title,
                        Content = this.content,
                        BackgroundColor = this.backgroundcolor,
                        Type = this.type,
                        Status = ChatHubRoomStatus.Enabled.ToString(),
                        ImageUrl = string.Empty,
                        OneVsOneId = string.Empty,
                        CreatorId = ChatHubService.ConnectedUser.UserId,
                    };

                    room = await this.ChatHubService.AddChatHubRoomAsync(room);
                    await logger.LogInformation("Room Added {ChatHubRoom}", room);
                    NavigationManager.NavigateTo(NavigateUrl());
                }
                else
                {                    
                    ChatHubRoom room = await this.ChatHubService.GetChatHubRoomAsync(roomId, ModuleState.ModuleId);
                    if (room != null)
                    {
                        room.Title = this.title;
                        room.Content = this.content;
                        room.BackgroundColor = this.backgroundcolor;
                        room.Type = this.type;

                        await this.ChatHubService.UpdateChatHubRoomAsync(room);

                        await logger.LogInformation("Room Updated {ChatHubRoom}", room);
                        NavigationManager.NavigateTo(NavigateUrl());
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Saving Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Saving Room", MessageType.Error);
            }
        }

        private void UpdateUIStateHasChanged()
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            this.BlazorColorPickerService.OnBlazorColorPickerContextColorChangedEvent -= OnBlazorColorPickerContextColorChangedExecute;
            this.ChatHubService.OnUpdateUI -= (object sender, EventArgs e) => UpdateUIStateHasChanged();
        }

    }
}
