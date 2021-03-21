using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorSelect
{
    public class BlazorSelectBase : ComponentBase
    {

        [Parameter] public HashSet<string> SelectionItems { get; set; }

        [Parameter] public string SelectedItem { get; set; }

        [Parameter] public EventCallback<BlazorSelectEvent> SelectEvent { get; set; }

        public void OnSelectionChange(ChangeEventArgs e)
        {
            InvokeAsync(() => this.SelectEvent.InvokeAsync(new BlazorSelectEvent() { SelectedItem = e.Value.ToString() }));
        }

    }
}
