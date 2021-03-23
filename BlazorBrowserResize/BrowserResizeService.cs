using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorBrowserResize
{
    public class BlazorBrowserResizeService : IAsyncDisposable
    {

        public IJSRuntime JsRuntime;
        public IJSObjectReference Module;
        public IJSObjectReference BrowserResizeMap;
        public DotNetObjectReference<BlazorBrowserResizeServiceExtension> DotNetObjRef;
        public BlazorBrowserResizeServiceExtension BrowserResizeServiceExtension;

        public BlazorBrowserResizeService(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.BrowserResizeServiceExtension = new BlazorBrowserResizeServiceExtension();
            this.DotNetObjRef = DotNetObjectReference.Create(this.BrowserResizeServiceExtension);
        }
        public async Task InitBrowserResizeService()
        {
            this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorBrowserResize/browserresizejsinterop.js");
            this.BrowserResizeMap = await this.Module.InvokeAsync<IJSObjectReference>("initbrowserresize", this.DotNetObjRef);
        }

        public void RegisterWindowResizeCallback()
        {
            this.BrowserResizeMap.InvokeVoidAsync("registerResizeCallback");
        }
        public async Task<int> GetInnerHeight()
        {
            return await this.BrowserResizeMap.InvokeAsync<int>("getInnerHeight");
        }
        public async Task<int> GetInnerWidth()
        {
            return await this.BrowserResizeMap.InvokeAsync<int>("getInnerWidth");
        }
        public async ValueTask DisposeAsync()
        {
            await this.BrowserResizeMap.DisposeAsync();
            await this.Module.DisposeAsync();
        }

    }

    public class BlazorBrowserResizeServiceExtension
    {

        public event Func<Task> OnResize;

        [JSInvokable("OnBrowserResize")]
        public async Task OnBrowserResize()
        {
            await OnResize?.Invoke();
        }

    }
}
