using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorTabs
{
    public partial class TabItemBase : ComponentBase, IDisposable, ITabItem
    {

        [CascadingParameter] public TabContainer TabContainer { get; set; }

        [Parameter] public RenderFragment TabTitle { get; set; }

        [Parameter] public RenderFragment TabContent { get; set; }

        [Parameter] public int Id { get; set; }

        [Parameter] public bool InitialSelection { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.TabContainer.AddTabItem(this);

            if (!this.TabContainer.InitialSelection)
            {
                this.TabContainer.ActiveTab = this;
                this.TabContainer.InitialSelection = true;
            }
            if (this.InitialSelection)
            {
                this.TabContainer.ActiveTab = this;
            }

            await base.OnInitializedAsync();
        }

        public string TitleCssClass => this.TabContainer.ActiveTab == this ? "active" : null;

        public void ActivateTab()
        {
            this.TabContainer.ActiveTab = this;
        }

        public void UpdateTabContent()
        {
            if (this.TabContainer.ActiveTab == this)
            {
                InvokeAsync(StateHasChanged);
            }
        }

        public void Dispose()
        {
            this.TabContainer.RemoveTabItem(this.Id);
        }

    }
}
