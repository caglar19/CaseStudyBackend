using System.Net;

namespace CaseStudy.Application.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ResponseContent { get; }

        public ApiException(HttpStatusCode statusCode, string message, string? responseContent = null) 
            : base(message)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        public static async Task ThrowIfNotSuccessfulAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new ApiException(
                    response.StatusCode,
                    $"API request failed with status code {(int)response.StatusCode}",
                    content
                );
            }
        }
    }
}
