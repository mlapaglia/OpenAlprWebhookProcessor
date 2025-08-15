using AwesomeAssertions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Utilities;
using System.Net;
using System.Text;

namespace Tests.Utilities
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class HttpResponseMessageExtensionsTests
    {
        [Test]
        public async Task EnsureSuccessWithDetailsAsync_WithSuccessStatusCode_ReturnsOriginalResponse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Success response", Encoding.UTF8, "application/json")
            };
            var errorMessagePrefix = "Test operation failed";

            // Act
            var result = await response.EnsureSuccessWithDetailsAsync(errorMessagePrefix);

            // Assert
            result.Should().Be(response);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task EnsureSuccessWithDetailsAsync_With201StatusCode_ReturnsOriginalResponse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("Created response", Encoding.UTF8, "application/json")
            };
            var errorMessagePrefix = "Test operation failed";

            // Act
            var result = await response.EnsureSuccessWithDetailsAsync(errorMessagePrefix);

            // Assert
            result.Should().Be(response);
            result.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Test]
        public async Task EnsureSuccessWithDetailsAsync_With204StatusCode_ReturnsOriginalResponse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            var errorMessagePrefix = "Test operation failed";

            // Act
            var result = await response.EnsureSuccessWithDetailsAsync(errorMessagePrefix);

            // Assert
            result.Should().Be(response);
            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Test]
        public void EnsureSuccessWithDetailsAsync_With400StatusCode_ThrowsHttpRequestExceptionWithDetails()
        {
            // Arrange
            var errorContent = "Bad request details";
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorContent, Encoding.UTF8, "application/json"),
                ReasonPhrase = "Bad Request"
            };
            var errorMessagePrefix = "API call failed";

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                async () => await response.EnsureSuccessWithDetailsAsync(errorMessagePrefix));

            exception.Message.Should().Contain(errorMessagePrefix);
            exception.Message.Should().Contain("400");
            exception.Message.Should().Contain("Bad Request");
            exception.Message.Should().Contain(errorContent);
        }

        [Test]
        public void EnsureSuccessWithDetailsAsync_With404StatusCode_ThrowsHttpRequestExceptionWithDetails()
        {
            // Arrange
            var errorContent = "Resource not found";
            var response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(errorContent, Encoding.UTF8, "text/plain"),
                ReasonPhrase = "Not Found"
            };
            var errorMessagePrefix = "Resource retrieval failed";

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                async () => await response.EnsureSuccessWithDetailsAsync(errorMessagePrefix));

            exception.Message.Should().Contain(errorMessagePrefix);
            exception.Message.Should().Contain("404");
            exception.Message.Should().Contain("Not Found");
            exception.Message.Should().Contain(errorContent);
        }

        [Test]
        public void EnsureSuccessWithDetailsAsync_With500StatusCode_ThrowsHttpRequestExceptionWithDetails()
        {
            // Arrange
            var errorContent = "Internal server error occurred";
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(errorContent, Encoding.UTF8, "application/json"),
                ReasonPhrase = "Internal Server Error"
            };
            var errorMessagePrefix = "Server operation failed";

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                async () => await response.EnsureSuccessWithDetailsAsync(errorMessagePrefix));

            exception.Message.Should().Contain(errorMessagePrefix);
            exception.Message.Should().Contain("500");
            exception.Message.Should().Contain("Internal Server Error");
            exception.Message.Should().Contain(errorContent);
        }

        [Test]
        public void EnsureSuccessWithDetailsAsync_WithEmptyErrorContent_IncludesEmptyContentInMessage()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("", Encoding.UTF8, "application/json"),
                ReasonPhrase = "Bad Request"
            };
            var errorMessagePrefix = "Test failed";

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(
                async () => await response.EnsureSuccessWithDetailsAsync(errorMessagePrefix));

            exception.Message.Should().Contain(errorMessagePrefix);
            exception.Message.Should().Contain("400");
            exception.Message.Should().Contain("Bad Request");
            exception.Message.Should().Contain("Response:");
        }

        [Test]
        public async Task EnsureSuccessWithDetailsAsync_WithCancellationToken_PassesToReadAsStringAsync()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Success", Encoding.UTF8, "application/json")
            };
            var errorMessagePrefix = "Test operation";

            // Act
            var result = await response.EnsureSuccessWithDetailsAsync(errorMessagePrefix, cts.Token);

            // Assert
            result.Should().Be(response);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup any resources if needed
        }
    }
}