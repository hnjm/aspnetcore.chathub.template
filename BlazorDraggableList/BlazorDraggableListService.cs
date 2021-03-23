using Microsoft.JSInterop;
using Oqtane.Shared.Models;
using System;
using System.Threading.Tasks;

namespace BlazorDraggableList
{
    public class BlazorDraggableListService : IBlazorDraggableListService
    {

        private readonly IJSRuntime JSRuntime;
        public IJSObjectReference Module;
        public IJSObjectReference DraggableListMap;

        public JsRuntimeObjectRef __jsRuntimeObjectRef { get; set; }

        public BlazorDraggableListServiceExtension BlazorDraggableListServiceExtension;

        public DotNetObjectReference<BlazorDraggableListServiceExtension> dotNetObjectReference;

        public BlazorDraggableListService(IJSRuntime jsRuntime)
        {
            this.JSRuntime = jsRuntime;
            this.BlazorDraggableListServiceExtension = new BlazorDraggableListServiceExtension();
            this.dotNetObjectReference = DotNetObjectReference.Create(this.BlazorDraggableListServiceExtension);
        }

        public async Task InitDraggableList(string elementId)
        {
            this.Module = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorDraggableList/blazordraggablelistjsinterop.js");
            this.DraggableListMap = await this.Module.InvokeAsync<IJSObjectReference>("initblazordraggablelist", this.dotNetObjectReference, elementId);
        }

    }

    public class BlazorDraggableListServiceExtension
    {

        public event EventHandler<BlazorDraggableListEvent> OnDropEvent;

        [JSInvokable("OnDrop")]
        public void OnDrop(int oldIndex, int newIndex, string elementId)
        {
            if (oldIndex >= 0 && newIndex >= 0)
            {
                BlazorDraggableListEvent eventParameters = new BlazorDraggableListEvent() { DraggableItemOldIndex = oldIndex, DraggableItemNewIndex = newIndex, DraggableContainerElementId = elementId };
                OnDropEvent?.Invoke(this, eventParameters);
            }
        }

    }
}
