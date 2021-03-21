using Microsoft.AspNetCore.Components.Routing;

namespace BlazorTabMenu
{
    public class BlazorTabMenuItemModel
    {

        public string Name { get; set; }

        public string Href { get; set; }

        public string Icon { get; set; }

        public NavLinkMatch NavLinkMatch { get; set; }

    }
}
