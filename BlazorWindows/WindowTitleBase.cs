using Microsoft.AspNetCore.Components;

namespace BlazorWindows
{
    public partial class WindowTitleBase : ComponentBase
    {

        [CascadingParameter]
        public IWindowItem WindowItem { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

    }
}
