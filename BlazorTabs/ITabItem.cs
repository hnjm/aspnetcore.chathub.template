using Microsoft.AspNetCore.Components;

namespace BlazorTabs
{
    public interface ITabItem
    {

        TabContainer TabContainer { get; set; }
        RenderFragment TabTitle { get; set; }
        RenderFragment TabContent { get; set; }
        int Id { get; set; }
        void UpdateTabContent();

    }
}
