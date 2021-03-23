using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorVideo
{
    public class BlazorVideoComponentBase : ComponentBase
    {

        [Inject] public BlazorVideoService BlazorVideoService { get; set; }

        [Parameter] public string Id { get; set; }

    }
}
