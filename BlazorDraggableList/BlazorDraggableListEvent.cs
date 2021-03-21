namespace BlazorDraggableList
{
    public class BlazorDraggableListEvent
    {

        public int DraggableItemNewIndex { get; set; }

        public int DraggableItemOldIndex { get; set; }

        public string DraggableContainerElementId { get; set; }

        public BlazorDraggableListEvent() { }

    }
}
