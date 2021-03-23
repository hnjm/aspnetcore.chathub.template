using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorVideo
{

    public class BlazorVideoService : IAsyncDisposable
    {

        public IJSRuntime _jSRuntime;
        public IJSObjectReference _moduleTask;
        public IJSObjectReference _blazorvideomap;
        public DotNetObjectReference<BlazorVideoServiceExtension> _dotnetobjectref;
        public BlazorVideoServiceExtension BlazorVideoServiceExtension;

        public BlazorVideoService(IJSRuntime jsRuntime)
        {
            this._jSRuntime = jsRuntime;
            this.BlazorVideoServiceExtension = new BlazorVideoServiceExtension();
            this._dotnetobjectref = DotNetObjectReference.Create(this.BlazorVideoServiceExtension);
        }

        public async Task InitBlazorVideo()
        {
            this._moduleTask = await this._jSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorVideo/blazorvideojsinterop.js");
            this._blazorvideomap = await this._moduleTask.InvokeAsync<IJSObjectReference>("initblazorvideo", this._dotnetobjectref);
        }

        public async ValueTask NewVideo(string id)
        {
            await this._blazorvideomap.InvokeVoidAsync("newvideo", id);
        }

        public async ValueTask DisposeAsync()
        {
            //await this._blazorvideomap.DisposeAsync();
            //await this._moduleTask.DisposeAsync();
        }
    }

    public class BlazorVideoServiceExtension
    {
        public event EventHandler<dynamic> OnDataAvailableEventHandler;

        [JSInvokable("OnDataAvailable")]
        public void OnDataAvailable(string dataURI, int id)
        {
            this.OnDataAvailableEventHandler?.Invoke(typeof(BlazorVideoServiceExtension), new { dataURI = dataURI, id = id });
        }
    }

}
