using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Utilities
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Throws an HttpRequestException with detailed error information if the response is not successful
        /// </summary>
        /// <param name="response">The HttpResponseMessage to check</param>
        /// <param name="errorMessagePrefix">The prefix message describing what operation failed</param>
        /// <param name="cancellationToken">Cancellation token for reading response content</param>
        /// <returns>The original response for method chaining</returns>
        /// <exception cref="HttpRequestException">Thrown when the response is not successful</exception>
        public static async Task<HttpResponseMessage> EnsureSuccessWithDetailsAsync(
            this HttpResponseMessage response,
            string errorMessagePrefix,
            CancellationToken cancellationToken = default)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorMessage = $"{errorMessagePrefix}: " +
                    $"Status: {(int)response.StatusCode} {response.StatusCode} {response.ReasonPhrase}, " +
                    $"Response: {errorContent}";

                throw new HttpRequestException(errorMessage);
            }

            return response;
        }
    }
}
