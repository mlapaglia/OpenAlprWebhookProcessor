using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyRegistration;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Tests.TestHelpers;
using System.Security.Claims;
using Fido2NetLib;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class CompletePasskeyRegistrationCommandHandlerTests : TestBase
    {
        private CompletePasskeyRegistrationCommandHandler _handler;
        private UserManager<ApplicationUser> _userManager;
        private IFido2 _mockFido2;
        private IMemoryCache _memoryCache;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            // Set up UserManager with the test context
            var services = new ServiceCollection();
            services.AddSingleton(UsersContext);
            services.AddLogging();
            
            services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<UsersContext>()
                .AddDefaultTokenProviders();
            
            var serviceProvider = services.BuildServiceProvider();
            _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Mock dependencies
            _mockFido2 = Substitute.For<IFido2>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            
            _handler = new CompletePasskeyRegistrationCommandHandler(
                _userManager, 
                _mockFido2, 
                UsersContext, 
                _memoryCache);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            _memoryCache?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_UserNotFound_ThrowsAppException()
        {
            // Arrange
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "nonexistentuser"),
                new Claim(ClaimTypes.NameIdentifier, "999")
            }));

            var command = new CompletePasskeyRegistrationCommand(userClaims, "{}", "MyPasskey");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_InvalidAttestationResponse_ReturnsFailureResponse()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var command = new CompletePasskeyRegistrationCommand(userClaims, "invalid-json", "MyPasskey");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().StartWith("Failed to register passkey:");
        }

        [Test]
        public async Task Handle_ExpiredRegistrationSession_ReturnsFailureResponse()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            // Valid JSON but no cached options
            var attestationResponseJson = """
                {
                    "id": "AQIDBAU",
                    "rawId": "AQIDBAU",
                    "type": "public-key",
                    "response": {
                        "attestationObject": "BgcI",
                        "clientDataJSON": "CQoL"
                    }
                }
                """;

            var command = new CompletePasskeyRegistrationCommand(userClaims, attestationResponseJson, "MyPasskey");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Registration session expired or invalid");
        }

        [Test]
        public async Task Handle_CacheIsClearedOnSessionExpiry()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            // Set up cached options
            var originalOptions = new CredentialCreateOptions
            {
                Challenge = new byte[] { 1, 2, 3 }
            };
            var optionsCacheKey = $"passkey_registration_{user.Id}";
            _memoryCache.Set(optionsCacheKey, originalOptions);

            var attestationResponseJson = """
                {
                    "id": "AQIDBAU",
                    "rawId": "AQIDBAU",
                    "type": "public-key",
                    "response": {
                        "attestationObject": "BgcI",
                        "clientDataJSON": "CQoL"
                    }
                }
                """;

            var command = new CompletePasskeyRegistrationCommand(userClaims, attestationResponseJson, "MyPasskey");
            var cancellationToken = GetCancellationToken();

            // Verify cache contains options before
            _memoryCache.Get<CredentialCreateOptions>(optionsCacheKey).Should().NotBeNull();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            
            // Verify cache was cleared (this happens after retrieving options)
            var cachedOptions = _memoryCache.Get<CredentialCreateOptions>(optionsCacheKey);
            cachedOptions.Should().BeNull();
        }

        [Test]
        public async Task Handle_NullAttestationResponse_ReturnsFailureResponse()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var command = new CompletePasskeyRegistrationCommand(userClaims, "null", "MyPasskey");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().StartWith("Failed to register passkey:");
        }
    }
}