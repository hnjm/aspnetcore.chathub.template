using Microsoft.AspNetCore.Components.Routing;

namespace BlazorTouchMenu
{
    public class BlazorTouchMenuItemModel
    {

        public string Name { get; set; }

        public string Href { get; set; }

        public string Icon { get; set; }

        public NavLinkMatch NavLinkMatch { get; set; }

    }
}
