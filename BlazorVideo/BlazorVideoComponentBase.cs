using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorVideo
{
    public class BlazorVideoComponentBase : ComponentBase, IDisposable
    {

        [Inject] public BlazorVideoService BlazorVideoService { get; set; }
        [Parameter] public string Id { get; set; }
        [Parameter] public string Name { get; set; }
        [Parameter] public BlazorVideoType Type { get; set; }
        [Parameter] public string BackgroundColor { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.BlazorVideoService.RunUpdateUI += UpdateUIStateHasChanged;

            await this.BlazorVideoService.InitBlazorVideo(this.Id, this.Type);
            await this.BlazorVideoService.InitBlazorVideoMap(this.Id, this.Type);
            await this.BlazorVideoService.InitJsLivestreams(this.Id, this.Type);
            await this.BlazorVideoService.InitDevicesLocalLivestream(this.Id);

            await base.OnInitializedAsync();
        }

        private void UpdateUIStateHasChanged()
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            this.BlazorVideoService.RunUpdateUI -= UpdateUIStateHasChanged;
        }

    }
}
