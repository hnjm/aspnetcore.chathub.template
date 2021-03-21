using Microsoft.AspNetCore.Components.Forms;

namespace BlazorFileUpload
{
    public class BlazorFileUploadModel
    {

        public string Base64ImageUrl { get; set; }

        public IBrowserFile BrowserFile { get; set; }

    }
}
