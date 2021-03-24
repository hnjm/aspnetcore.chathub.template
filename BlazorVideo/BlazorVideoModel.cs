using Microsoft.JSInterop;

namespace BlazorVideo
{
    public class BlazorVideoModel
    {

        public string Id { get; set; }

        public BlazorVideoType Type { get; set; }

        public IJSObjectReference JsObjRef { get; set; }

    }
}
