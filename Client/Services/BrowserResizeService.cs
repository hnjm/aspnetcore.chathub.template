using Microsoft.JSInterop;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Services
{
    public class BrowserResizeService : ServiceBase, IService
    {

        public JsRuntimeObjectRef __jsRuntimeObjectRef { get; set; }

        private readonly IJSRuntime JSRuntime;

        public event Func<Task> OnResize;

        public DotNetObjectReference<BrowserResizeService> dotNetObjectReference;

        public BrowserResizeService(HttpClient http, IJSRuntime JSRuntime) : base(http)
        {
            this.JSRuntime = JSRuntime;
            this.dotNetObjectReference = DotNetObjectReference.Create<BrowserResizeService>(this);
        }

        [JSInvokable("OnBrowserResize")]
        public async Task OnBrowserResize()
        {
            await OnResize?.Invoke();
        }

        public async Task RegisterWindowResizeCallback()
        {
            await this.JSRuntime.InvokeVoidAsync("__obj.browserResize.registerResizeCallback", __jsRuntimeObjectRef);
        }

        public async Task<int> GetInnerHeight()
        {
            return await this.JSRuntime.InvokeAsync<int>("__obj.browserResize.getInnerHeight", __jsRuntimeObjectRef);
        }

        public async Task<int> GetInnerWidth()
        {
            return await this.JSRuntime.InvokeAsync<int>("__obj.browserResize.getInnerWidth", __jsRuntimeObjectRef);
        }
    }
}