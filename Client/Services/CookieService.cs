using Microsoft.JSInterop;
using Oqtane.ChatHubs.Client.Video;
using Oqtane.Modules;
using Oqtane.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs
{
    public class CookieService : ServiceBase, IService, IVideoService
    {

        private readonly IJSRuntime JSRuntime;

        public CookieService(HttpClient httpClient, IJSRuntime JSRuntime) : base(httpClient)
        {
            this.JSRuntime = JSRuntime;
        }

        public async Task<string> GetCookieAsync(string cookieName)
        {
            return await this.JSRuntime.InvokeAsync<string>("cookies.getCookie", cookieName);
        }

        public async Task SetCookie(string cookieName, string cookieValue, int expirationDays)
        {
            await this.JSRuntime.InvokeVoidAsync("cookies.setCookie", cookieName);
        }

    }
}
