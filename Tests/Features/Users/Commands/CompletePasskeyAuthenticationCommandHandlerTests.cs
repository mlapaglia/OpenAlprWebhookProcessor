using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyAuthentication;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Tests.TestHelpers;
using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class CompletePasskeyAuthenticationCommandHandlerTests : TestBase
    {
        private CompletePasskeyAuthenticationCommandHandler _handler;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
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
            
            // Add HttpContext support for SignInManager
            var httpContext = Substitute.For<HttpContext>();
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            httpContextAccessor.HttpContext.Returns(httpContext);
            services.AddSingleton(httpContextAccessor);
            
            // Add authentication services required by SignInManager
            services.AddAuthentication()
                .AddCookie();
            
            // Mock authentication service in HttpContext
            var authService = Substitute.For<IAuthenticationService>();
            var mockServiceProvider = Substitute.For<IServiceProvider>();
            mockServiceProvider.GetService(typeof(IAuthenticationService)).Returns(authService);
            httpContext.RequestServices.Returns(mockServiceProvider);
            
            services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<UsersContext>()
                .AddDefaultTokenProviders();
            
            var serviceProvider = services.BuildServiceProvider();
            _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _signInManager = serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

            // Mock dependencies
            _mockFido2 = Substitute.For<IFido2>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            
            _handler = new CompletePasskeyAuthenticationCommandHandler(
                _userManager, 
                _signInManager, 
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
        public async Task Handle_ValidAuthentication_ReturnsUserDto()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            // Create a passkey credential
            var credentialId = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
            var credential = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = credentialId,
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 1,
                CredType = "public-key",
                RegDate = DateTime.UtcNow,
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Test Passkey"
            };
            
            UsersContext.PasskeyCredentials.Add(credential);
            await UsersContext.SaveChangesAsync();

            // Mock assertion response
            var assertionResponse = new AuthenticatorAssertionRawResponse
            {
                Id = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }),
                RawId = new byte[] { 1, 2, 3, 4, 5 },
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = new byte[] { 7, 8, 9 },
                    ClientDataJson = new byte[] { 10, 11, 12 },
                    Signature = new byte[] { 13, 14, 15 }
                }
            };

            var assertionResponseJson = JsonSerializer.Serialize(assertionResponse);

            // Mock cached options
            var originalOptions = new AssertionOptions
            {
                Challenge = new byte[] { 16, 17, 18 },
                RpId = "test.com",
                AllowCredentials = new List<PublicKeyCredentialDescriptor>
                {
                    new PublicKeyCredentialDescriptor(new byte[] { 1, 2, 3, 4, 5 })
                }
            };

            var optionsCacheKey = $"passkey_authentication_{user.Id}";
            _memoryCache.Set(optionsCacheKey, originalOptions);

            // Mock successful FIDO2 verification
            var makeAssertionResult = new VerifyAssertionResult
            {
                SignCount = 2
            };

            _mockFido2.MakeAssertionAsync(
                Arg.Any<MakeAssertionParams>(),
                Arg.Any<CancellationToken>())
                .Returns(makeAssertionResult);

            var command = new CompletePasskeyAuthenticationCommand("testuser", assertionResponseJson, false);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("testuser");
            result.FirstName.Should().Be("Test");
            result.LastName.Should().Be("User");
            result.Id.Should().Be(user.Id);
            result.TwoFactorEnabled.Should().BeFalse();
            result.HasPasskeys.Should().BeTrue();

            // Verify cache was cleared
            _memoryCache.Get<AssertionOptions>(optionsCacheKey).Should().BeNull();

            // Verify signature counter was updated
            var updatedCredential = UsersContext.PasskeyCredentials.First(c => c.Id == credential.Id);
            updatedCredential.SignatureCounter.Should().Be(2u);
        }

        [Test]
        public async Task Handle_UserNotFound_ThrowsAppException()
        {
            // Arrange
            var command = new CompletePasskeyAuthenticationCommand("nonexistentuser", "{}", false);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_InvalidAssertionResponse_ThrowsAppException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var command = new CompletePasskeyAuthenticationCommand("testuser", "invalid-json", false);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Passkey authentication failed: *");
        }

        [Test]
        public async Task Handle_CredentialNotFound_ThrowsAppException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var assertionResponse = new AuthenticatorAssertionRawResponse
            {
                Id = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }),
                RawId = new byte[] { 1, 2, 3, 4, 5 },
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = new byte[] { 7, 8, 9 },
                    ClientDataJson = new byte[] { 10, 11, 12 },
                    Signature = new byte[] { 13, 14, 15 }
                }
            };

            var assertionResponseJson = JsonSerializer.Serialize(assertionResponse);
            var command = new CompletePasskeyAuthenticationCommand("testuser", assertionResponseJson, false);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("*Credential not found*");
        }

        [Test]
        public async Task Handle_ExpiredAuthenticationSession_ThrowsAppException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var credentialId = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
            var credential = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = credentialId,
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 1,
                CredType = "public-key",
                RegDate = DateTime.UtcNow,
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Test Passkey"
            };
            
            UsersContext.PasskeyCredentials.Add(credential);
            await UsersContext.SaveChangesAsync();

            var assertionResponse = new AuthenticatorAssertionRawResponse
            {
                Id = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }),
                RawId = new byte[] { 1, 2, 3, 4, 5 },
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = new byte[] { 7, 8, 9 },
                    ClientDataJson = new byte[] { 10, 11, 12 },
                    Signature = new byte[] { 13, 14, 15 }
                }
            };

            var assertionResponseJson = JsonSerializer.Serialize(assertionResponse);

            // Mock missing cached options (expired session)
            var optionsCacheKey = $"passkey_authentication_{user.Id}";
            // Don't set anything in cache to simulate expired session

            var command = new CompletePasskeyAuthenticationCommand("testuser", assertionResponseJson, false);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("*Authentication session expired or invalid. Please try again.*");
        }

        [Test]
        public async Task Handle_FailedFido2Verification_ThrowsAppException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var credentialId = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
            var credential = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = credentialId,
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 1,
                CredType = "public-key",
                RegDate = DateTime.UtcNow,
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Test Passkey"
            };
            
            UsersContext.PasskeyCredentials.Add(credential);
            await UsersContext.SaveChangesAsync();

            var assertionResponse = new AuthenticatorAssertionRawResponse
            {
                Id = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }),
                RawId = new byte[] { 1, 2, 3, 4, 5 },
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = new byte[] { 7, 8, 9 },
                    ClientDataJson = new byte[] { 10, 11, 12 },
                    Signature = new byte[] { 13, 14, 15 }
                }
            };

            var assertionResponseJson = JsonSerializer.Serialize(assertionResponse);

            var originalOptions = new AssertionOptions
            {
                Challenge = new byte[] { 16, 17, 18 },
                RpId = "test.com"
            };

            var optionsCacheKey = $"passkey_authentication_{user.Id}";
            _memoryCache.Set(optionsCacheKey, originalOptions);

            // Mock failed FIDO2 verification - throw exception in v4.0.0
            _mockFido2.MakeAssertionAsync(
                Arg.Any<MakeAssertionParams>(),
                Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Invalid signature"));

            var command = new CompletePasskeyAuthenticationCommand("testuser", assertionResponseJson, false);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("*Passkey authentication failed: Invalid signature*");
        }

        [Test]
        public async Task Handle_RememberMeTrue_PassesRememberMeToSignIn()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var credentialId = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
            var credential = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = credentialId,
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 1,
                CredType = "public-key",
                RegDate = DateTime.UtcNow,
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Test Passkey"
            };
            
            UsersContext.PasskeyCredentials.Add(credential);
            await UsersContext.SaveChangesAsync();

            var assertionResponse = new AuthenticatorAssertionRawResponse
            {
                Id = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }),
                RawId = new byte[] { 1, 2, 3, 4, 5 },
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = new byte[] { 7, 8, 9 },
                    ClientDataJson = new byte[] { 10, 11, 12 },
                    Signature = new byte[] { 13, 14, 15 }
                }
            };

            var assertionResponseJson = JsonSerializer.Serialize(assertionResponse);

            var originalOptions = new AssertionOptions
            {
                Challenge = new byte[] { 16, 17, 18 },
                RpId = "test.com"
            };

            var optionsCacheKey = $"passkey_authentication_{user.Id}";
            _memoryCache.Set(optionsCacheKey, originalOptions);

            var makeAssertionResult = new VerifyAssertionResult
            {
                SignCount = 2
            };

            _mockFido2.MakeAssertionAsync(
                Arg.Any<MakeAssertionParams>(),
                Arg.Any<CancellationToken>())
                .Returns(makeAssertionResult);

            var command = new CompletePasskeyAuthenticationCommand("testuser", assertionResponseJson, true);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("testuser");
        }
    }
}
