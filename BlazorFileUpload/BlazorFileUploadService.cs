using Microsoft.JSInterop;
using Oqtane.Shared.Models;
using System;
using System.Threading.Tasks;

namespace BlazorFileUpload
{
    public class BlazorFileUploadService
    {

        public IJSRuntime JSRuntime;
        public IJSObjectReference Module;
        public IJSObjectReference FileUploadMap;

        public JsRuntimeObjectRef __jsRuntimeObjectRef { get; set; }

        public BlazorFileUploadServiceExtension BlazorFileUploadServiceExtension;

        public DotNetObjectReference<BlazorFileUploadServiceExtension> dotNetObjectReference;

        public BlazorFileUploadService(IJSRuntime jsRuntime)
        {
            this.JSRuntime = jsRuntime;
            this.BlazorFileUploadServiceExtension = new BlazorFileUploadServiceExtension();
            this.dotNetObjectReference = DotNetObjectReference.Create(this.BlazorFileUploadServiceExtension);
        }

        public async Task InitFileUploadDropzone(string inputFileId, string elementId)
        {
            this.Module = await this.JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorFileUpload/blazorfileuploadjsinterop.js");
            this.FileUploadMap = await this.Module.InvokeAsync<IJSObjectReference>("initfileupload", this.dotNetObjectReference, inputFileId, elementId);
        }

    }

    public class BlazorFileUploadServiceExtension
    {

        public event EventHandler<BlazorFileUploadEvent> OnDropEvent;

        [JSInvokable("OnDrop")]
        public void OnDrop(string elementId)
        {
            BlazorFileUploadEvent eventParameters = new BlazorFileUploadEvent() { FileUploadDropzoneId = elementId };
            OnDropEvent?.Invoke(this, eventParameters);
        }

    }
}
