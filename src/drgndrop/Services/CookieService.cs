using Microsoft.JSInterop;

namespace drgndrop.Services
{
    public class CookieService
    {
        public Dictionary<string, string> Cookies { get; set; } = new Dictionary<string, string>();

        public async Task LoadCookies(IJSRuntime js)
        {
            Cookies = new Dictionary<string, string>();
            string cookieString = await js.InvokeAsync<string>("cookies.read");
            if (!string.IsNullOrEmpty(cookieString))
            {
                var splitCookies = cookieString.Split(";");
                foreach (var cookie in splitCookies)
                {
                    var splitCookie = cookie.Split("=");
                    Cookies.Add(splitCookie[0], splitCookie[1]);
                }
            }
        }

        public string? GetCookie(string key)
        {
            if(Cookies.TryGetValue(key, out var value)) return value;
            return null;
        }

        public async Task WriteCookie(IJSRuntime js, string key, string value, int days)
        {
            await js.InvokeVoidAsync("cookies.write", key, value, days);
            if(!Cookies.ContainsKey(key)) Cookies.Add(key, value);
            else Cookies[key] = value;
        }

        public async Task DeleteCookie(IJSRuntime js, string key)
        {
            await js.InvokeVoidAsync("cookies.delete", key);
            if (Cookies.ContainsKey(key)) Cookies.Remove(key);
        }
    }
}
