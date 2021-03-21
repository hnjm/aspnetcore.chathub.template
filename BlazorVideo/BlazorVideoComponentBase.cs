using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorVideo
{
    public class BlazorVideoComponentBase : ComponentBase
    {

        [Inject] public BlazorVideoService BlazorVideoService { get; set; }

        [Parameter] public string Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                /*
                this.BlazorVideoService._moduleTask = await this.BlazorVideoService._jSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorVideo/blazorvideojsinterop.js");
                this.BlazorVideoService.BlazorVideoServiceExtension = new BlazorVideoServiceExtension();
                this.BlazorVideoService._dotnetobjectref = DotNetObjectReference.Create(this.BlazorVideoService.BlazorVideoServiceExtension);
                this.BlazorVideoService._blazorvideomap = await this.BlazorVideoService._moduleTask.InvokeAsync<IJSObjectReference>("initblazorvideo", this.BlazorVideoService._dotnetobjectref);
                */
            }
        }

    }
}
