using Microsoft.JSInterop;
using Oqtane.Shared.Models;
using System;

namespace BlazorDraggableList
{
    public class BlazorDraggableListService : IBlazorDraggableListService
    {

        private readonly IJSRuntime JSRuntime;

        public JsRuntimeObjectRef __jsRuntimeObjectRef { get; set; }

        public BlazorDraggableListServiceExtension BlazorDraggableListServiceExtension;

        public DotNetObjectReference<BlazorDraggableListServiceExtension> dotNetObjectReference;

        public BlazorDraggableListService(IJSRuntime jsRuntime)
        {
            this.JSRuntime = jsRuntime;

            this.BlazorDraggableListServiceExtension = new BlazorDraggableListServiceExtension(this);
            this.dotNetObjectReference = DotNetObjectReference.Create(this.BlazorDraggableListServiceExtension);
        }

        public void InitDraggable(string elementId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                this.JSRuntime.InvokeVoidAsync("__obj.initdraggable", elementId, __jsRuntimeObjectRef);
            }
        }

    }

    public class BlazorDraggableListServiceExtension
    {

        private BlazorDraggableListService BlazorDraggableListService { get; set; }

        public event EventHandler<BlazorDraggableListEvent> OnDropEvent;

        public BlazorDraggableListServiceExtension(BlazorDraggableListService blazorDraggableListService)
        {
            this.BlazorDraggableListService = blazorDraggableListService;
        }

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
