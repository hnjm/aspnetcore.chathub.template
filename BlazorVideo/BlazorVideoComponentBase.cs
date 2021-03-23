using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorVideo
{
    public class BlazorVideoComponentBase : ComponentBase
    {

        [Inject] public BlazorVideoService BlazorVideoService { get; set; }

        [Parameter] public string Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await this.BlazorVideoService.InitBlazorVideo(this.Id);
            await base.OnInitializedAsync();
        }

    }
}
