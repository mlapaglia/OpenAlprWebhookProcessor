using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.TestHelpers
{
    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage _responseMessage;
        private Exception _exception;

        public Uri LastRequestUri { get; private set; }
        public HttpMethod LastRequestMethod { get; private set; }
        public HttpContent LastRequestContent { get; private set; }

        public void SetupResponse(HttpStatusCode statusCode, byte[] content = null)
        {
            _responseMessage = new HttpResponseMessage(statusCode);
            if (content != null)
            {
                _responseMessage.Content = new ByteArrayContent(content);
            }
            _exception = null;
        }

        public void SetupException(Exception exception)
        {
            _exception = exception;
            _responseMessage = null;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            LastRequestMethod = request.Method;
            LastRequestContent = request.Content;

            if (_exception != null)
            {
                throw _exception;
            }

            return Task.FromResult(_responseMessage ?? new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
} 