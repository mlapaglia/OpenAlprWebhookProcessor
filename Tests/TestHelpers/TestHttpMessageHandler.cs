using System.Net;
namespace Tests.TestHelpers
{
    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responseMessages = new();
        private Exception _exception;

        public Uri LastRequestUri { get; private set; }
        public HttpMethod LastRequestMethod { get; private set; }
        public HttpContent LastRequestContent { get; private set; }
        public List<string> RequestLog { get; } = new();

        // Additional properties for compatibility with HikvisionCameraTests
        public HttpMethod RequestMethod => LastRequestMethod;
        public Uri RequestUri => LastRequestUri;
        public string RequestContent { get; private set; }
        
        private TimeSpan? _delay;

        public void SetupResponse(HttpStatusCode statusCode, byte[] content = null)
        {
            var responseMessage = new HttpResponseMessage(statusCode);
            if (content != null)
            {
                responseMessage.Content = new ByteArrayContent(content);
            }
            _responseMessages.Enqueue(responseMessage);
            _exception = null;
            _delay = null;
        }

        public void SetupResponse(HttpStatusCode statusCode, string content)
        {
            var responseMessage = new HttpResponseMessage(statusCode);
            responseMessage.Content = new StringContent(content ?? "");
            _responseMessages.Enqueue(responseMessage);
            _exception = null;
            _delay = null;
        }

        public void SetupDelayedResponse(HttpStatusCode statusCode, string content, TimeSpan delay)
        {
            var responseMessage = new HttpResponseMessage(statusCode);
            responseMessage.Content = new StringContent(content ?? "");
            _responseMessages.Enqueue(responseMessage);
            _exception = null;
            _delay = delay;
        }

        public void SetupException(Exception exception)
        {
            _exception = exception;
            _responseMessages.Clear();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            LastRequestUri = request.RequestUri;
            LastRequestMethod = request.Method;
            LastRequestContent = request.Content;

            // Capture request content as string for compatibility
            if (request.Content != null)
            {
                RequestContent = await request.Content.ReadAsStringAsync();
            }

            var requestInfo = $"{request.Method} {request.RequestUri}";
            RequestLog.Add(requestInfo);

            if (_exception != null)
            {
                throw _exception;
            }

            // Handle delay if specified
            if (_delay.HasValue)
            {
                await Task.Delay(_delay.Value, cancellationToken);
            }

            HttpResponseMessage response;
            if (_responseMessages.Count > 0)
            {
                response = _responseMessages.Dequeue();
            }
            else
            {
                response = new HttpResponseMessage(HttpStatusCode.OK);
            }

            // Log response details for debugging
            if (response.Content != null)
            {
                var contentStr = await response.Content.ReadAsStringAsync();
                RequestLog.Add($"Response: {response.StatusCode}, Content: {contentStr}");
                // Reset content for actual consumption
                response.Content = new StringContent(contentStr);
            }
            else
            {
                RequestLog.Add($"Response: {response.StatusCode}, Content: null");
            }

            return response;
        }
    }
} 