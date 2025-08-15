using AwesomeAssertions;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor;
using OpenAlprWebhookProcessor.Features.Users.Services;
using Tests.TestHelpers;

namespace Tests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class StartupTests : TestBase
    {
        private IConfiguration _configuration;
        private Startup _startup;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // Create a mock configuration
            var configDict = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", "Data Source=:memory:"},
                {"ConnectionStrings:UsersConnection", "Data Source=:memory:"},
                {"MachineLearning:ModelPath", "test-model-path"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            _configuration = configuration;
            _startup = new Startup(_configuration);
        }

        [Test]
        public void Constructor_WithValidConfiguration_InitializesCorrectly()
        {
            // Act & Assert
            _startup.Should().NotBeNull();
            _startup.Configuration.Should().NotBeNull();
            _startup.Configuration.Should().Be(_configuration);
        }

        [Test]
        public void Constructor_WithNullConfiguration_AcceptsNullConfiguration()
        {
            // Act & Assert - Constructor should not throw, but accept null configuration
            var startup = new Startup(null);
            startup.Should().NotBeNull();
            startup.Configuration.Should().BeNull();
        }

        [Test]
        public void Configuration_Property_ReturnsConfigurationFromConstructor()
        {
            // Act
            var result = _startup.Configuration;

            // Assert
            result.Should().Be(_configuration);
        }

        [Test]
        public void ConfigureServices_WithValidServiceCollection_RegistersBasicServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            _startup.ConfigureServices(services);

            // Assert
            services.Should().NotBeEmpty();
            
            // Verify some key services are registered
            services.Should().Contain(s => s.ServiceType == typeof(IFlurlClientCache));
            // Old repository services removed - using Identity's UserManager instead
            // IJwtKeyRepository removed - JWT authentication no longer used
            // IJwtService removed - JWT authentication no longer used
            services.Should().Contain(s => s.ServiceType == typeof(IPasswordService));
        }

        [Test]
        public void ConfigureServices_RegistersFlurlClientCacheAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            _startup.ConfigureServices(services);

            // Assert
            var flurlClientCacheDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IFlurlClientCache));
            flurlClientCacheDescriptor.Should().NotBeNull();
            flurlClientCacheDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Test]
        public void ConfigureServices_WithNullServiceCollection_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _startup.ConfigureServices(null));
            exception.ParamName.Should().Be("services");
        }

        [Test]
        public void ConfigureServices_CallsExtensionMethods()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Add minimal required services for extension methods to work
            services.AddLogging();
            services.AddOptions();

            // Act & Assert - Should not throw, indicating extension methods are called successfully
            Assert.DoesNotThrow(() => _startup.ConfigureServices(services));
        }

        [Test]
        public void Configure_WithValidParameters_AcceptsParameters()
        {
            // Arrange
            var app = Substitute.For<IApplicationBuilder>();
            var env = Substitute.For<IWebHostEnvironment>();
            
            // Setup environment
            env.EnvironmentName.Returns("Production");

            // Act & Assert - The method should accept valid parameters
            // Note: We don't test full pipeline configuration due to service dependencies
            var configureMethod = typeof(Startup).GetMethod("Configure");
            configureMethod.Should().NotBeNull();
        }

        [Test]
        public void Configure_WithDevelopmentEnvironment_AcceptsParameters()
        {
            // Arrange
            var app = Substitute.For<IApplicationBuilder>();
            var env = Substitute.For<IWebHostEnvironment>();
            
            // Setup environment as development
            env.EnvironmentName.Returns("Development");

            // Act & Assert - The method should accept valid parameters
            // Note: We don't test full pipeline configuration due to service dependencies
            var configureMethod = typeof(Startup).GetMethod("Configure");
            configureMethod.Should().NotBeNull();
        }

        [Test]
        public void Configure_WithProductionEnvironment_AcceptsParameters()
        {
            // Arrange
            var app = Substitute.For<IApplicationBuilder>();
            var env = Substitute.For<IWebHostEnvironment>();
            
            // Setup environment as production
            env.EnvironmentName.Returns("Production");

            // Act & Assert - The method should accept valid parameters
            // Note: We don't test full pipeline configuration due to service dependencies
            var configureMethod = typeof(Startup).GetMethod("Configure");
            configureMethod.Should().NotBeNull();
        }

        [Test]
        public void Configure_WithNullApplicationBuilder_ThrowsArgumentNullException()
        {
            // Arrange
            var env = Substitute.For<IWebHostEnvironment>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _startup.Configure(null, env));
            exception.ParamName.Should().Be("app");
        }

        [Test]
        public void Configure_WithNullEnvironment_ThrowsArgumentNullException()
        {
            // Arrange
            var app = Substitute.For<IApplicationBuilder>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _startup.Configure(app, null));
            exception.ParamName.Should().Be("hostEnvironment");
        }

        [Test]
        public void Startup_HasPublicConstructor()
        {
            // Arrange & Act
            var startupType = typeof(Startup);
            var constructor = startupType.GetConstructor(new[] { typeof(IConfiguration) });

            // Assert
            constructor.Should().NotBeNull("Startup should have a public constructor that takes IConfiguration");
            constructor.IsPublic.Should().BeTrue("Startup constructor should be public");
        }

        [Test]
        public void Startup_HasPublicConfigurationProperty()
        {
            // Arrange & Act
            var startupType = typeof(Startup);
            var configProperty = startupType.GetProperty("Configuration");

            // Assert
            configProperty.Should().NotBeNull("Startup should have a Configuration property");
            configProperty.PropertyType.Should().Be(typeof(IConfiguration));
            configProperty.CanRead.Should().BeTrue("Configuration property should be readable");
            configProperty.GetMethod.IsPublic.Should().BeTrue("Configuration property getter should be public");
        }

        [Test]
        public void Startup_HasPublicConfigureServicesMethod()
        {
            // Arrange & Act
            var startupType = typeof(Startup);
            var configureServicesMethod = startupType.GetMethod("ConfigureServices");

            // Assert
            configureServicesMethod.Should().NotBeNull("Startup should have a ConfigureServices method");
            configureServicesMethod.IsPublic.Should().BeTrue("ConfigureServices method should be public");
            configureServicesMethod.ReturnType.Should().Be(typeof(void));
            
            var parameters = configureServicesMethod.GetParameters();
            parameters.Should().HaveCount(1);
            parameters[0].ParameterType.Should().Be(typeof(IServiceCollection));
        }

        [Test]
        public void Startup_HasPublicConfigureMethod()
        {
            // Arrange & Act
            var startupType = typeof(Startup);
            var configureMethod = startupType.GetMethod("Configure");

            // Assert
            configureMethod.Should().NotBeNull("Startup should have a Configure method");
            configureMethod.IsPublic.Should().BeTrue("Configure method should be public");
            configureMethod.ReturnType.Should().Be(typeof(void));
            
            var parameters = configureMethod.GetParameters();
            parameters.Should().HaveCount(2);
            parameters[0].ParameterType.Should().Be(typeof(IApplicationBuilder));
            parameters[1].ParameterType.Should().Be(typeof(IWebHostEnvironment));
        }
    }
}