using System.Text.Json;
using System.Web;

namespace CaseStudy.Application.Extensions
{
    public static class HttpExtensions
    {
        public static string AddQueryParameter(this string url, string key, string value)
        {
            if (string.IsNullOrEmpty(value)) return url;

            var separator = url.Contains("?") ? "&" : "?";
            return $"{url}{separator}{key}={HttpUtility.UrlEncode(value)}";
        }

        public static string AddQueryParameter(this string url, string key, int? value)
        {
            if (!value.HasValue) return url;
            return url.AddQueryParameter(key, value.Value.ToString());
        }

        public static async Task<T> DeserializeApiResponseAsync<T>(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content);
        }
    }
}
