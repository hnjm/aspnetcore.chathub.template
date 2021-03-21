using Microsoft.JSInterop;
using Oqtane.Modules;
using Oqtane.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Services
{
    public class ScrollService : ServiceBase, IService
    {

        private readonly IJSRuntime JSRuntime;

        public ScrollService(HttpClient httpClient, IJSRuntime jsRuntime) : base(httpClient)
        {
            this.JSRuntime = jsRuntime;
        }

        public async Task ScrollToBottom(string element)
        {
            await this.JSRuntime.InvokeAsync<object>("scroll.scrollToBottom", element);
        }

    }
}