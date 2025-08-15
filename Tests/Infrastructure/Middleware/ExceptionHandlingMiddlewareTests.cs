using AwesomeAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Infrastructure.Middleware;
using System.Text.Json;

namespace Tests.Infrastructure.Middleware
{
    [TestFixture]
    public class ExceptionHandlingMiddlewareTests
    {
        private ExceptionHandlingMiddleware _middleware;
        private ILogger<ExceptionHandlingMiddleware> _logger;
        private RequestDelegate _next;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void SetUp()
        {
            _logger = NSubstitute.Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
            _next = NSubstitute.Substitute.For<RequestDelegate>();
            _middleware = new ExceptionHandlingMiddleware(_next, _logger);
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new MemoryStream();
        }

        [Test]
        public async Task InvokeAsync_WithNoException_ShouldCallNext()
        {
            // Arrange
            _next.When(x => x(_httpContext)).Do(x => { });

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            await _next.Received(1).Invoke(_httpContext);
        }

        [Test]
        public async Task InvokeAsync_WithValidationException_ShouldReturnBadRequest()
        {
            // Arrange
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new("PropertyName", "Error message")
            };
            var validationException = new ValidationException(validationFailures);
            _next.When(x => x(_httpContext)).Do(x => throw validationException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(400);
            _httpContext.Response.ContentType.Should().Be("application/json");

            _httpContext.Response.Body.Position = 0;
            var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            response.GetProperty("error").GetProperty("message").GetString().Should().Be("Validation failed");
            response.GetProperty("error").TryGetProperty("details", out var details).Should().BeTrue();
        }

        [Test]
        public async Task InvokeAsync_WithArgumentException_ShouldReturnBadRequest()
        {
            // Arrange
            var argumentException = new ArgumentException("Invalid argument");
            _next.When(x => x(_httpContext)).Do(x => throw argumentException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(400);
            _httpContext.Response.ContentType.Should().Be("application/json");

            _httpContext.Response.Body.Position = 0;
            var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            response.GetProperty("error").GetProperty("message").GetString().Should().Be("Invalid argument");
        }

        [Test]
        public async Task InvokeAsync_WithArgumentExceptionWithInnerException_ShouldIncludeInnerMessage()
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner exception message");
            var argumentException = new ArgumentException("Outer exception message", innerException);
            _next.When(x => x(_httpContext)).Do(x => throw argumentException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(400);

            _httpContext.Response.Body.Position = 0;
            var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            response.GetProperty("error").GetProperty("message").GetString().Should().Be("Outer exception message");
            response.GetProperty("error").GetProperty("details").GetString().Should().Be("Inner exception message");
        }

        [Test]
        public async Task InvokeAsync_WithUnauthorizedAccessException_ShouldReturnUnauthorized()
        {
            // Arrange
            var unauthorizedException = new UnauthorizedAccessException("Access denied");
            _next.When(x => x(_httpContext)).Do(x => throw unauthorizedException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(401);
            _httpContext.Response.ContentType.Should().Be("application/json");

            _httpContext.Response.Body.Position = 0;
            var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            response.GetProperty("error").GetProperty("message").GetString().Should().Be("Access denied");
        }

        [Test]
        public async Task InvokeAsync_WithNotImplementedException_ShouldReturnNotImplemented()
        {
            // Arrange
            var notImplementedException = new NotImplementedException("Feature not implemented");
            _next.When(x => x(_httpContext)).Do(x => throw notImplementedException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(501);
            _httpContext.Response.ContentType.Should().Be("application/json");

            _httpContext.Response.Body.Position = 0;
            var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            response.GetProperty("error").GetProperty("message").GetString().Should().Be("Feature not implemented");
        }

        [Test]
        public async Task InvokeAsync_WithGenericException_ShouldReturnInternalServerError()
        {
            // Arrange
            var genericException = new InvalidOperationException("Something went wrong");
            _next.When(x => x(_httpContext)).Do(x => throw genericException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(500);
            _httpContext.Response.ContentType.Should().Be("application/json");

            _httpContext.Response.Body.Position = 0;
            var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            response.GetProperty("error").GetProperty("message").GetString().Should().Be("An internal server error occurred");
        }

        [Test]
        public async Task InvokeAsync_WithGenericExceptionWithInnerException_ShouldIncludeInnerMessage()
        {
            // Arrange
            var innerException = new ArgumentException("Inner exception");
            var genericException = new InvalidOperationException("Outer exception", innerException);
            _next.When(x => x(_httpContext)).Do(x => throw genericException);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(500);

            _httpContext.Response.Body.Position = 0;
            var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
            
            response.GetProperty("error").GetProperty("message").GetString().Should().Be("An internal server error occurred");
            response.GetProperty("error").GetProperty("details").GetString().Should().Be("Inner exception");
        }

        [Test]
        public async Task InvokeAsync_WithAnyException_ShouldLogError()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            _next.When(x => x(_httpContext)).Do(x => throw exception);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("An unhandled exception occurred")),
                exception,
                Arg.Any<Func<object, Exception, string>>()
            );
        }

        [Test]
        public async Task InvokeAsync_ShouldSetContentTypeToApplicationJson()
        {
            // Arrange
            var exception = new ArgumentException("Test exception");
            _next.When(x => x(_httpContext)).Do(x => throw exception);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.ContentType.Should().Be("application/json");
        }

        [Test]
        public async Task InvokeAsync_ShouldReturnValidJsonResponse()
        {
            // Arrange
            var exception = new ArgumentException("Test exception");
            _next.When(x => x(_httpContext)).Do(x => throw exception);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _httpContext.Response.Body.Position = 0;
            var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();

            // Should be valid JSON
            JsonElement response = default;
            var isValidJson = false;
            try
            {
                response = JsonSerializer.Deserialize<JsonElement>(responseBody);
                isValidJson = true;
            }
            catch (JsonException)
            {
                isValidJson = false;
            }
            isValidJson.Should().BeTrue();
            
            // Should have error object
            response.TryGetProperty("error", out var error).Should().BeTrue();
            error.TryGetProperty("message", out _).Should().BeTrue();
        }

        [Test]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var middleware = new ExceptionHandlingMiddleware(_next, _logger);

            // Assert
            middleware.Should().NotBeNull();
        }


    }
}