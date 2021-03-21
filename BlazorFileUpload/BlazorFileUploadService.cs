using Microsoft.JSInterop;
using Oqtane.Shared.Models;
using System;

namespace BlazorFileUpload
{
    public class BlazorFileUploadService
    {

        private readonly IJSRuntime JSRuntime;

        public JsRuntimeObjectRef __jsRuntimeObjectRef { get; set; }

        public BlazorFileUploadServiceExtension BlazorFileUploadServiceExtension;

        public DotNetObjectReference<BlazorFileUploadServiceExtension> dotNetObjectReference;

        public BlazorFileUploadService(IJSRuntime jsRuntime)
        {
            this.JSRuntime = jsRuntime;

            this.BlazorFileUploadServiceExtension = new BlazorFileUploadServiceExtension(this);
            this.dotNetObjectReference = DotNetObjectReference.Create(this.BlazorFileUploadServiceExtension);
        }

        public void InitFileUploadDropzone(string inputFileId, string elementId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                this.JSRuntime.InvokeVoidAsync("__obj.initfileuploaddropzone", inputFileId, elementId, __jsRuntimeObjectRef);
            }
        }

    }

    public class BlazorFileUploadServiceExtension
    {

        private BlazorFileUploadService BlazorFileUploadService { get; set; }

        public event EventHandler<BlazorFileUploadEvent> OnDropEvent;

        public BlazorFileUploadServiceExtension(BlazorFileUploadService blazorFileUploadService)
        {
            this.BlazorFileUploadService = blazorFileUploadService;
        }

        [JSInvokable("OnDrop")]
        public void OnDrop(string elementId)
        {
            BlazorFileUploadEvent eventParameters = new BlazorFileUploadEvent() { FileUploadDropzoneId = elementId };
            OnDropEvent?.Invoke(this, eventParameters);
        }

    }
}
