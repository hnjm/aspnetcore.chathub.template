using Microsoft.JSInterop;
using Oqtane.ChatHubs.Client.Video;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs
{
    public class VideoService : ServiceBase, IService, IVideoService
    {

        public JsRuntimeObjectRef __jsRuntimeObjectRef { get; set; }

        public VideoServiceExtension VideoServiceExtension;
        public DotNetObjectReference<VideoServiceExtension> dotNetObjectReference;

        private readonly HttpClient HttpClient;
        private readonly IJSRuntime JSRuntime;
        
        public event EventHandler<int> OnContinueLivestreamTask;

        public VideoService(HttpClient httpClient, IJSRuntime JSRuntime) : base(httpClient)
        {
            this.HttpClient = httpClient;
            this.JSRuntime = JSRuntime;

            this.VideoServiceExtension = new VideoServiceExtension();
            this.dotNetObjectReference = DotNetObjectReference.Create(this.VideoServiceExtension);
        }

        public async Task StartBroadcasting(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.startbroadcasting", roomId, this.__jsRuntimeObjectRef);
            }
        }

        public async Task StartStreaming(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.startstreaming", roomId, __jsRuntimeObjectRef);
            }
        }

        public async Task CloseLivestream(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.closelivestream", roomId, this.__jsRuntimeObjectRef);
            }
        }

        public async Task StartSequence(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.startsequence", roomId, this.__jsRuntimeObjectRef);
            }
        }

        public async Task StopSequence(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.stopsequence", roomId, this.__jsRuntimeObjectRef);
            }
        }

        public async Task DrawImage(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.drawimage", roomId, this.__jsRuntimeObjectRef);
            }
        }

        public async Task AppendBuffer(string dataURI, int roomId, string dataType)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.appendbuffer", dataURI, roomId, dataType, this.__jsRuntimeObjectRef);
            }
        }

        public void ContinueLivestreamTask(int roomId)
        {
            this.OnContinueLivestreamTask?.Invoke(typeof(VideoService), roomId);
        }

    }

    public class VideoServiceExtension
    {

        public event EventHandler<dynamic> OnDataAvailableEventHandler;
        public event EventHandler<int> OnPauseLivestreamTask;

        [JSInvokable("OnDataAvailable")]
        public object OnDataAvailable(string dataURI, int roomId, string dataType)
        {
            this.OnDataAvailableEventHandler?.Invoke(typeof(VideoServiceExtension), new { dataURI = dataURI, roomId = roomId, dataType = dataType });
            return new { msg = string.Format("room id: {1}; dataType: {2}; dataUri: {0}", dataURI, roomId, dataType) };
        }

        [JSInvokable("PauseLivestreamTask")]
        public void PauseLivestreamTask(int roomId)
        {
            this.OnPauseLivestreamTask?.Invoke(typeof(VideoServiceExtension), roomId);
        }

    }
}
