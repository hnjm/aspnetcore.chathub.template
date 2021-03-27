using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BlazorVideo
{

    public class BlazorVideoService : IAsyncDisposable
    {

        public IDictionary<Guid, BlazorVideoModel> BlazorVideoMaps { get; set; } = new Dictionary<Guid, BlazorVideoModel>();
        public IJSObjectReference Module;
        public IJSRuntime JsRuntime;
        
        public DotNetObjectReference<BlazorVideoServiceExtension> DotNetObjectRef;
        public BlazorVideoServiceExtension BlazorVideoServiceExtension;

        public event Action RunUpdateUI;

        public Dictionary<string, dynamic> LocalStreamTasks { get; set; } = new Dictionary<string, dynamic>();
        public List<string> RemoteStreamTasks { get; set; } = new List<string>();

        public BlazorVideoService(IJSRuntime jsRuntime)
        {
            this.JsRuntime = jsRuntime;
            this.BlazorVideoServiceExtension = new BlazorVideoServiceExtension(this);
            this.DotNetObjectRef = DotNetObjectReference.Create(this.BlazorVideoServiceExtension);
        }
        public async Task InitBlazorVideo(string id, BlazorVideoType type)
        {
            this.Module = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorVideo/blazorvideojsinterop.js");
        }
        public async Task InitBlazorVideoMap(string id, BlazorVideoType type)
        {
            IJSObjectReference jsobjref = await this.Module.InvokeAsync<IJSObjectReference>("initblazorvideo", this.DotNetObjectRef, id, type.ToString().ToLower());
            this.AddBlazorVideoMap(id, type, jsobjref);
        }
        
        public void AddBlazorVideoMap(string id, BlazorVideoType type, IJSObjectReference jsobjref)
        {
            if (!this.BlazorVideoMaps.Any(item => item.Value.Id == id))
            {
                this.BlazorVideoMaps.Add(new KeyValuePair<Guid, BlazorVideoModel>(Guid.NewGuid(), new BlazorVideoModel() { Id = id, Type = type, JsObjRef = jsobjref, VideoOverlay = true }));
            }
        }
        public void RemoveBlazorVideoMap(Guid guid)
        {
            if (this.BlazorVideoMaps.Any(item => item.Key == guid))
            {
                this.BlazorVideoMaps.Remove(guid);
            }
        }

        public async Task InitLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            await keyvaluepair.Value.JsObjRef.InvokeVoidAsync("initlocallivestream");
        }
        public async Task InitDevicesLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            await keyvaluepair.Value.JsObjRef.InvokeVoidAsync("initdeviceslocallivestream");
        }
        public async Task StartBroadcastingLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            await keyvaluepair.Value.JsObjRef.InvokeVoidAsync("startbroadcastinglocallivestream");
        }
        public async Task StartSequenceLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            await keyvaluepair.Value.JsObjRef.InvokeVoidAsync("startsequencelocallivestream");
        }
        public async Task StopSequenceLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            await keyvaluepair.Value.JsObjRef.InvokeVoidAsync("stopsequencelocallivestream"); 
        }
        public async Task ContinueLocalLivestreamAsync(string id)
        {
            List<KeyValuePair<string, dynamic>> localList = this.LocalStreamTasks.Where(item => item.Key.ToString() == id).ToList();
            List<string> remoteList = this.RemoteStreamTasks.Where(item => item.ToString() == id).ToList();

            if (localList.Any() || remoteList.Any())
            {
                await this.StartVideoChat(id);
            }
        }
        public async Task CloseLocalLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            await keyvaluepair.Value.JsObjRef.InvokeVoidAsync("closelocallivestream");
        }

        public async Task InitRemoteLivestream(string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            await keyvaluepair.Value.JsObjRef.InvokeVoidAsync("initremotelivestream");
        }
        public async Task AppendBufferRemoteLivestream(string dataURI, string id)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == id);
            await keyvaluepair.Value.JsObjRef.InvokeVoidAsync("appendbufferremotelivestream", dataURI);
        }

        public async Task StartVideoChat(string roomId)
        {
            try
            {
                var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == roomId);
                await this.StopVideoChat(keyvaluepair.Value.Id);

                if (keyvaluepair.Value.Type == BlazorVideoType.LocalLivestream)
                {
                    //await this.InitLocalLivestream(keyvaluepair.Value.Id);
                    //await this.InitDevicesLocalLivestream(keyvaluepair.Value.Id);
                    await this.StartBroadcastingLocalLivestream(keyvaluepair.Value.Id);

                    this.BlazorVideoMaps[keyvaluepair.Key] = new BlazorVideoModel() { Id = keyvaluepair.Value.Id, JsObjRef = keyvaluepair.Value.JsObjRef, Type = keyvaluepair.Value.Type, VideoOverlay = false };
                    this.RunUpdateUI.Invoke();

                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    CancellationToken token = tokenSource.Token;
                    Task task = new Task(async () => await this.StreamTaskImplementation(keyvaluepair.Value.Id, token), token);
                    this.AddLocalStreamTask(keyvaluepair.Value.Id, task, tokenSource);
                    task.Start();
                }
                else if(keyvaluepair.Value.Type == BlazorVideoType.RemoteLivestream)
                {
                    this.AddRemoteStreamTask(roomId);

                    this.BlazorVideoMaps[keyvaluepair.Key] = new BlazorVideoModel() { Id = keyvaluepair.Value.Id, JsObjRef = keyvaluepair.Value.JsObjRef, Type = keyvaluepair.Value.Type, VideoOverlay = false };
                    this.RunUpdateUI.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public async Task StopVideoChat(string videoMap)
        {
            var keyvaluepair = this.BlazorVideoMaps.FirstOrDefault(item => item.Value.Id == videoMap);
            if (keyvaluepair.Value.Type == BlazorVideoType.LocalLivestream)
            {
                this.BlazorVideoMaps[keyvaluepair.Key] = new BlazorVideoModel() { Id = keyvaluepair.Value.Id, JsObjRef = keyvaluepair.Value.JsObjRef, Type = keyvaluepair.Value.Type, VideoOverlay = true };
                this.RunUpdateUI.Invoke();

                this.RemoveLocalStreamTask(videoMap);
                await this.CloseLocalLivestream(videoMap);
                await this.InitLocalLivestream(keyvaluepair.Value.Id);
                await this.InitDevicesLocalLivestream(keyvaluepair.Value.Id);
            }
            else if (keyvaluepair.Value.Type == BlazorVideoType.RemoteLivestream)
            {
                this.BlazorVideoMaps[keyvaluepair.Key] = new BlazorVideoModel() { Id = keyvaluepair.Value.Id, JsObjRef = keyvaluepair.Value.JsObjRef, Type = keyvaluepair.Value.Type, VideoOverlay = true };
                this.RunUpdateUI.Invoke();

                this.RemoveRemoteStreamTask(videoMap);
            }
        }
        public async Task RestartStreamTaskIfExists(string roomId)
        {
            if (this.LocalStreamTasks.Any(item => item.Key == roomId) || this.RemoteStreamTasks.Any(item => item == roomId))
            {
                await this.StartVideoChat(roomId);
            }

            this.RunUpdateUI.Invoke();
        }
        public void AddLocalStreamTask(string roomId, Task task, CancellationTokenSource tokenSource)
        {
            this.RemoveLocalStreamTask(roomId);
            dynamic obj = new { task = task, tokenSource = tokenSource };
            this.LocalStreamTasks.Add(roomId, obj);

            this.RunUpdateUI.Invoke();
        }
        public void RemoveLocalStreamTask(string roomId)
        {
            List<KeyValuePair<string, dynamic>> list = this.LocalStreamTasks.Where(item => item.Key == roomId).ToList();
            if (list.Any())
            {
                KeyValuePair<string, dynamic> keyValuePair = list.FirstOrDefault();
                dynamic obj = keyValuePair.Value;

                obj.tokenSource?.Cancel();
                obj.task?.Dispose();

                this.LocalStreamTasks.Remove(keyValuePair.Key);
            }
        }
        public void AddRemoteStreamTask(string roomId)
        {
            var items = this.RemoteStreamTasks.Where(id => id == roomId);
            if (!items.Any())
            {
                this.RemoteStreamTasks.Add(roomId);

                this.RunUpdateUI.Invoke();
            }
        }
        public void RemoveRemoteStreamTask(string roomId)
        {
            var items = this.RemoteStreamTasks.Where(id => id == roomId);
            if (items.Any())
            {
                this.RemoteStreamTasks.Remove(roomId);
            }
        }
        public async Task StreamTaskImplementation(string roomId, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await this.StopSequenceLocalLivestream(roomId);
                    await this.StartSequenceLocalLivestream(roomId);

                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        public async Task DisposeStreamTasksAsync()
        {
            foreach (var task in LocalStreamTasks)
            {
                await this.StopVideoChat(task.Key);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await this.DisposeStreamTasksAsync();

            foreach (var keyvaluepair in this.BlazorVideoMaps)
            {
                await keyvaluepair.Value.JsObjRef.DisposeAsync();
            }

            await this.Module.DisposeAsync();
        }

    }

    public class BlazorVideoServiceExtension
    {

        public BlazorVideoService BlazorVideoService { get; set; }
        public event Action<string ,string> OnDataAvailableEventHandler;

        public BlazorVideoServiceExtension(BlazorVideoService blazorVideoService)
        {
            this.BlazorVideoService = blazorVideoService;
        }

        [JSInvokable("OnDataAvailable")]
        public void OnDataAvailable(string dataURI, string id)
        {
            if(!string.IsNullOrEmpty(dataURI) && !string.IsNullOrEmpty(id))
            {
                this.OnDataAvailableEventHandler.Invoke(dataURI, id);
            }
        }

        [JSInvokable("PauseLivestreamTask")]
        public void PauseLivestreamTask(string id)
        {
            List<KeyValuePair<string, dynamic>> list = this.BlazorVideoService.LocalStreamTasks.Where(item => item.Key == id).ToList();
            if (list.Any())
            {
                KeyValuePair<string, dynamic> keyValuePair = list.FirstOrDefault();
                dynamic obj = keyValuePair.Value;
                obj.tokenSource.Cancel();
                obj.task.Dispose();
            }
        }

    }

}
