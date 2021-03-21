using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWindows
{
    public partial class WindowContainerBase : ComponentBase
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public bool RenderLivestreams { get; set; }
        [Parameter] public string DraggableLivestreamContainerElementId { get; set; }

        [Parameter] public EventCallback<WindowEvent> ShowEvent { get; set; }
        [Parameter] public EventCallback<WindowEvent> ShownEvent { get; set; }
        [Parameter] public EventCallback<WindowEvent> HideEvent { get; set; }
        [Parameter] public EventCallback<WindowEvent> HiddenEvent { get; set; }
        [Parameter] public EventCallback<WindowEvent> AddedEvent { get; set; }
        [Parameter] public EventCallback<WindowEvent> RemovedEvent { get; set; }

        private bool Disposing { get; set; } = false;
        private bool HasRendered { get; set; } = false;
        private List<EventCallback<WindowEvent>> WindowEvents { get; set; } = new List<EventCallback<WindowEvent>>();
        private WindowEvent WindowEvent { get; set; }

        public bool InitialSelection { get; set; }

        private IWindowItem _activeWindow;
        public IWindowItem ActiveWindow
        {
            get { 
                return _activeWindow;
            }
            set
            {
                if (this.ActiveWindow == value) return;
                if (this.Disposing) return;
                this.WindowEvent = new WindowEvent() { ActivatedItem = value, DeactivatedItem = _activeWindow };

                if (this.HasRendered)
                {
                    InvokeAsync(() => ShowEvent.InvokeAsync(this.WindowEvent));
                    InvokeAsync(() => HideEvent.InvokeAsync(this.WindowEvent));
                    this.WindowEvents.Add(this.ShownEvent);
                    this.WindowEvents.Add(this.HiddenEvent);
                }
                this._activeWindow = value;
                InvokeAsync(StateHasChanged);
            }
        }

        public List<IWindowItem> WindowItems { get; set; } = new List<IWindowItem>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this.HasRendered = true;
            }

            for (var i = 0; i < this.WindowEvents.Count; i++)
            {
                await this.WindowEvents[i].InvokeAsync(this.WindowEvent);
                this.WindowEvents.RemoveAt(i);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public void SelectWindowById(int id)
        {
            this.ActiveWindow = this.WindowItems.Find(item => item.Id == id);
        }        

        public void AddWindowItem(IWindowItem windowItem)
        {
            if (!WindowItems.Any(item => item.Id == windowItem.Id))
            {
                InvokeAsync(() =>
                {
                    this.WindowItems.Add(windowItem);
                    this.WindowEvents.Add(AddedEvent);

                    StateHasChanged();
                });
            }
        }

        public void RemoveWindowItem(int id)
        {
            var windowItem = this.WindowItems.Where(item => item.Id == id).FirstOrDefault();
            if (windowItem != null)
            {
                InvokeAsync(() =>
                {
                    this.WindowItems.Remove(windowItem);
                    this.WindowEvents.Add(RemovedEvent);

                    StateHasChanged();
                });
            }
        }

    }
}
