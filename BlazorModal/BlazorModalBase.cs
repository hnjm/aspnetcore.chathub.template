using Microsoft.AspNetCore.Components;

namespace BlazorModal
{
    public class BlazorModalBase : ComponentBase
    {

        [Parameter] public RenderFragment BlazorModalHeader { get; set; }
        [Parameter] public RenderFragment BlazorModalBody { get; set; }
        [Parameter] public RenderFragment BlazorModalFooter { get; set; }

        [Parameter] public string ElementId { get; set; }

    }
}
