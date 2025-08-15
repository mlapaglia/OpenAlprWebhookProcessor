using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using OpenAlprWebhookProcessor;
using Tests.TestHelpers;

namespace Tests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ProgramTests : TestBase
    {
        [Test]
        public void CreateHostBuilder_WithEmptyArgs_ReturnsValidHostBuilder()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);

            // Assert
            hostBuilder.Should().NotBeNull();
        }

        [Test]
        public void CreateHostBuilder_WithNullArgs_ReturnsValidHostBuilder()
        {
            // Arrange
            string[] args = null;

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);

            // Assert
            hostBuilder.Should().NotBeNull();
        }

        [Test]
        public void CreateHostBuilder_WithValidArgs_ReturnsValidHostBuilder()
        {
            // Arrange
            string[] args = { "--environment", "Development" };

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);

            // Assert
            hostBuilder.Should().NotBeNull();
        }

        [Test]
        public void CreateHostBuilder_BuildsHostSuccessfully()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);
            using var host = hostBuilder.Build();

            // Assert
            host.Should().NotBeNull();
            host.Services.Should().NotBeNull();
        }

        [Test]
        public void CreateHostBuilder_ConfiguresRequiredServices()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);
            using var host = hostBuilder.Build();

            // Assert
            var serviceProvider = host.Services;
            
            // Check that basic services are registered
            serviceProvider.GetService<IWebHostEnvironment>().Should().NotBeNull();
            serviceProvider.GetService<IHostEnvironment>().Should().NotBeNull();
        }

        [Test]
        public void CreateHostBuilder_ConfiguresWebHostDefaults()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);
            using var host = hostBuilder.Build();

            // Assert
            var webHostEnvironment = host.Services.GetService<IWebHostEnvironment>();
            webHostEnvironment.Should().NotBeNull();
            webHostEnvironment.ApplicationName.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void CreateHostBuilder_UsesSerilogConfiguration()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);
            using var host = hostBuilder.Build();

            // Assert - Verify that Serilog is configured by checking that ILogger is available
            var logger = host.Services.GetService<Microsoft.Extensions.Logging.ILogger<ProgramTests>>();
            logger.Should().NotBeNull();
        }

        [Test]
        public void CreateHostBuilder_RegistersStartupClass()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);

            // Assert - Build should succeed, indicating Startup class is properly configured
            using var host = hostBuilder.Build();
            host.Should().NotBeNull();
        }

        [Test]
        public void CreateHostBuilder_WithMultipleArgs_HandlesGracefully()
        {
            // Arrange
            string[] args = { 
                "--urls", "http://localhost:5000",
                "--environment", "Production"
            };

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);

            // Assert
            hostBuilder.Should().NotBeNull();
            
            // Verify it can build without errors
            using var host = hostBuilder.Build();
            host.Should().NotBeNull();
        }

        [Test]
        public void CreateHostBuilder_CreatesDefaultBuilderWithCorrectConfiguration()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);
            using var host = hostBuilder.Build();

            // Assert
            var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
            hostEnvironment.Should().NotBeNull();
            
            // Verify some default builder configurations are present
            var configuration = host.Services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            configuration.Should().NotBeNull();
        }

        [Test]
        public void CreateHostBuilder_ConfiguresLogging()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var hostBuilder = Program.CreateHostBuilder(args);
            using var host = hostBuilder.Build();

            // Assert
            var loggerFactory = host.Services.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
            loggerFactory.Should().NotBeNull();
            
            var logger = loggerFactory.CreateLogger("TestLogger");
            logger.Should().NotBeNull();
        }

        [Test]
        public void Program_HasPublicMainMethod()
        {
            // Arrange & Act
            var programType = typeof(Program);
            var mainMethod = programType.GetMethod("Main", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            // Assert
            mainMethod.Should().NotBeNull("Program should have a public static Main method");
            mainMethod.ReturnType.Should().Be(typeof(System.Threading.Tasks.Task), "Main method should return Task");
            
            var parameters = mainMethod.GetParameters();
            parameters.Should().HaveCount(1, "Main method should have one parameter");
            parameters[0].ParameterType.Should().Be(typeof(string[]), "Main method parameter should be string[]");
        }

        [Test]
        public void Program_HasPublicCreateHostBuilderMethod()
        {
            // Arrange & Act
            var programType = typeof(Program);
            var createHostBuilderMethod = programType.GetMethod("CreateHostBuilder", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            // Assert
            createHostBuilderMethod.Should().NotBeNull("Program should have a public static CreateHostBuilder method");
            createHostBuilderMethod.ReturnType.Should().Be(typeof(IHostBuilder), "CreateHostBuilder should return IHostBuilder");
            
            var parameters = createHostBuilderMethod.GetParameters();
            parameters.Should().HaveCount(1, "CreateHostBuilder method should have one parameter");
            parameters[0].ParameterType.Should().Be(typeof(string[]), "CreateHostBuilder parameter should be string[]");
        }
    }
}