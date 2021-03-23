using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BlazorVideo
{

    public class BlazorVideoService : IAsyncDisposable
    {

        public IDictionary<string, IJSObjectReference> BlazorVideoMaps { get; set; } = new Dictionary<string, IJSObjectReference>();
        public IJSObjectReference Module;
        public IJSRuntime JsRuntime;
        
        public DotNetObjectReference<BlazorVideoServiceExtension> DotNetObjectRef;
        public BlazorVideoServiceExtension BlazorVideoServiceExtension;

        public BlazorVideoService(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.BlazorVideoServiceExtension = new BlazorVideoServiceExtension();
            this.DotNetObjectRef = DotNetObjectReference.Create(this.BlazorVideoServiceExtension);
        }

        public async Task InitBlazorVideo(string id)
        {
            this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorVideo/blazorvideojsinterop.js");
            IJSObjectReference jsobjref = await this.Module.InvokeAsync<IJSObjectReference>("initblazorvideo", this.DotNetObjectRef, id);

            this.AddDicItem(id, jsobjref);
        }

        public void AddDicItem(string id, IJSObjectReference jsobjref)
        {
            if(!this.BlazorVideoMaps.Any(item => item.Key == id))
            {
                this.BlazorVideoMaps.Add(new KeyValuePair<string, IJSObjectReference>(id, jsobjref));
            }
        }

        public void RemoveDicItem(string id)
        {
            if (this.BlazorVideoMaps.Any(item => item.Key == id))
            {
                this.BlazorVideoMaps.Remove(id);
            }
        }

        public void NewVideo(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Key == id);
            keyvaluepair.Value.InvokeVoidAsync("newvideo");
        }

        public async ValueTask DisposeAsync()
        {
            foreach(var keyvaluepair in this.BlazorVideoMaps)
            {
                await keyvaluepair.Value.DisposeAsync();
            }

            await this.Module.DisposeAsync();
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
