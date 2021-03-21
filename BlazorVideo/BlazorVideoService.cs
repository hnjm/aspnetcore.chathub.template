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
        }

        public async ValueTask NewVideo(string id)
        {
            await this._blazorvideomap.InvokeVoidAsync("newvideo", id);
        }

        public async ValueTask DisposeAsync()
        {
            await this._blazorvideomap.DisposeAsync();
            await this._moduleTask.DisposeAsync();            
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
