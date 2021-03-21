using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Oqtane.Shared.Models
{
    public class JsRuntimeObjectRef
    {
        [JsonPropertyName("__jsObjectRefId")]
        public int JsObjectRefId { get; set; }
    }
}
