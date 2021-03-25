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

        public event EventHandler<string> OnContinueLivestreamTask;

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

            this.AddBlazorVideoMap(id, type, jsobjref);
            this.InitJsMapLocalLivestream(id);
        }

        public void AddBlazorVideoMap(string id, BlazorVideoType type, IJSObjectReference jsobjref)
        {
            if(!this.BlazorVideoMaps.Any(item => item.Value.Id == id))
            {
                this.BlazorVideoMaps.Add(new KeyValuePair<Guid, BlazorVideoModel>(Guid.NewGuid(), new BlazorVideoModel() { Id = id, Type = type, JsObjRef = jsobjref }));
            }
        }
        public void RemoveBlazorVideoMap(Guid guid)
        {
            if (this.BlazorVideoMaps.Any(item => item.Key== guid))
            {
                this.BlazorVideoMaps.Remove(guid);
            }
        }

        public void InitJsMapLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            keyvaluepair.Value.JsObjRef.InvokeVoidAsync("initlocallivestream");
        }
        public void StartBroadcastingLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            this.BlazorVideoMaps[keyvaluepair.Key] = new BlazorVideoModel() { Id = keyvaluepair.Value.Id, JsObjRef = keyvaluepair.Value.JsObjRef, Type = keyvaluepair.Value.Type, VideoOverlay = true };
            keyvaluepair.Value.JsObjRef.InvokeVoidAsync("startbroadcastinglocallivestream");
        }
        public void StartSequenceLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            keyvaluepair.Value.JsObjRef.InvokeVoidAsync("startsequence", id);
        }
        public void StopSequenceLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            keyvaluepair.Value.JsObjRef.InvokeVoidAsync("stopsequence", id);
        }
        public void ContinueLocalLivestream(string id)
        {
            this.OnContinueLivestreamTask?.Invoke(typeof(BlazorVideoService), id);
        }
        public void CloseLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            keyvaluepair.Value.JsObjRef.InvokeVoidAsync("closelivestream", id);
        }

        public void StartStreamingRemoteLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            keyvaluepair.Value.JsObjRef.InvokeVoidAsync("startstreaming", id);
        }
        public void AppendBufferRemoteLivestream(string dataURI, string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            keyvaluepair.Value.JsObjRef.InvokeVoidAsync("appendbuffer", dataURI, id);
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
        public event EventHandler<string> OnPauseLivestreamTask;

        [JSInvokable("OnDataAvailable")]
        public void OnDataAvailable(string dataURI, int id)
        {
            this.OnDataAvailableEventHandler?.Invoke(typeof(BlazorVideoServiceExtension), new { dataURI = dataURI, id = id });
        }

        [JSInvokable("PauseLivestreamTask")]
        public void PauseLivestreamTask(string id)
        {
            this.OnPauseLivestreamTask?.Invoke(typeof(BlazorVideoServiceExtension), id);
        }

    }

}
