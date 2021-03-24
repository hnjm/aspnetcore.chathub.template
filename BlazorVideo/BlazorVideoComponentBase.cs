using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorVideo
{
    public class BlazorVideoComponentBase : ComponentBase
    {

        [Inject] public BlazorVideoService BlazorVideoService { get; set; }
        [Parameter] public string Id { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public BlazorVideoType Type { get; set; }
        [Parameter] public string BackgroundColor { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await this.BlazorVideoService.InitBlazorVideo(this.Id, this.Type);
            await base.OnInitializedAsync();
        }

    }
}
