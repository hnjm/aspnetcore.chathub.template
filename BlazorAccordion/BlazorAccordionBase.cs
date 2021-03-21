using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorAccordion
{
    public class BlazorAccordionBase : ComponentBase
    {

        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public string Id { get; set; }

        public IList<BlazorAccordionItem> AccordionItems { get; set; } = new List<BlazorAccordionItem>();

        public BlazorAccordionItem ActiveAccordionItem { get; set; }

        public void AddAccordionItem(BlazorAccordionItem item)
        {
            if(!this.AccordionItems.Contains(item))
            {
                this.AccordionItems.Add(item);
            }
        }

    }
}
