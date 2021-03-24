using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BlazorVideo
{

    public class BlazorVideoService : IAsyncDisposable
    {

        public IDictionary<Guid, BlazorVideoModel> BlazorVideoMaps { get; set; } = new Dictionary<Guid, BlazorVideoModel>();
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

        public async Task InitBlazorVideo(string id, BlazorVideoType type)
        {
            this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorVideo/blazorvideojsinterop.js");
            IJSObjectReference jsobjref = await this.Module.InvokeAsync<IJSObjectReference>("initblazorvideo", this.DotNetObjectRef, id, type.ToString().ToLower());

            this.AddDicItem(id, type, jsobjref);
        }

        public void AddDicItem(string id, BlazorVideoType type, IJSObjectReference jsobjref)
        {
            if(!this.BlazorVideoMaps.Any(item => item.Value.Id == id))
            {
                this.BlazorVideoMaps.Add(new KeyValuePair<Guid, BlazorVideoModel>(Guid.NewGuid(), new BlazorVideoModel() { Id = id, Type = type, JsObjRef = jsobjref }));
            }
        }

        public void RemoveDicItem(Guid guid)
        {
            if (this.BlazorVideoMaps.Any(item => item.Key== guid))
            {
                this.BlazorVideoMaps.Remove(guid);
            }
        }

        public void NewLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            keyvaluepair.Value.JsObjRef.InvokeVoidAsync("newlocallivestream");
        }

        public async ValueTask DisposeAsync()
        {
            foreach(var keyvaluepair in this.BlazorVideoMaps)
            {
                await keyvaluepair.Value.JsObjRef.DisposeAsync();
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
