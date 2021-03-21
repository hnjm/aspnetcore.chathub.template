using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorColorPicker
{
    public class BlazorColorPickerComponentBase : ComponentBase
    {

        [Inject] public BlazorColorPickerService BlazorColorPickerService { get; set; }

        [Parameter] public string ContextColor { get; set; }

        [Parameter] public BlazorColorPickerType ColorPickerType { get; set; }

        [Parameter]
        public Dictionary<string, dynamic> ColorSet { get; set; } = new Dictionary<string, dynamic>()
        {
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#dbdbdb" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#e5e5e5" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#eeeAeA" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#eee0F5" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#e8e8ee" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#ede5e6" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#e5eeea" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#e0eeee" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#e6e6ea" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#e0eee0" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#e5e5e5" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#eef9d8" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#eae7e0" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#ceafec" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#eaead2" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#eaf0e6" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#eaebd7" } },
            { Guid.NewGuid().ToString(), new { itemchecked = false, itemcolor = "#eeceaf" } },
        };

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        public void ContextColor_OnChange(string color)
        {
            this.ContextColor = color;
            this.BlazorColorPickerService.InvokeColorPickerEvent(color);
        }

        public void ColorSetItem_OnClicked(KeyValuePair<string, dynamic> clickedkvpair)
        {
            foreach(var checkedkvpair in this.ColorSet.Where(item => item.Value.itemchecked == true))
            {
                this.ColorSet[checkedkvpair.Key] = new { itemchecked = false, itemcolor = checkedkvpair.Value.itemcolor };
            }

            this.ColorSet[clickedkvpair.Key] = new { itemchecked = true, itemcolor = clickedkvpair.Value.itemcolor };
            this.ContextColor = clickedkvpair.Value.itemcolor;
            this.BlazorColorPickerService.InvokeColorPickerEvent(clickedkvpair.Value.itemcolor);
            this.StateHasChanged();
        }

        public bool ShowCustomColorPicker { get; set; }
        public void ToggleCustomColorPicker()
        {
            this.ShowCustomColorPicker = !this.ShowCustomColorPicker;
        }

    }
}
