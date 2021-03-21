using System.Text.Json.Serialization;

namespace Oqtane.Shared.Models
{
    public class JsRuntimeObjectRef
    {
        [JsonPropertyName("__jsObjectRefId")]
        public int JsObjectRefId { get; set; }
    }
}
