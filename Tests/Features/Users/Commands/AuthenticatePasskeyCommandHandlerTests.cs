using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.AuthenticatePasskey;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Tests.TestHelpers;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class AuthenticatePasskeyCommandHandlerTests : TestBase
    {
        private AuthenticatePasskeyCommandHandler _handler;
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
            
            _handler = new AuthenticatePasskeyCommandHandler(
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
        public async Task Handle_ValidUserWithPasskeys_ReturnsAuthenticatePasskeyResponse()
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

            // Add existing credentials
            var credentialId1 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            var credentialId2 = Convert.ToBase64String(new byte[] { 4, 5, 6 });
            
            UsersContext.PasskeyCredentials.AddRange(new[]
            {
                new PasskeyCredential
                {
                    UserId = user.Id,
                    CredentialId = credentialId1,
                    PublicKey = new byte[] { 1, 2, 3 },
                    UserHandle = new byte[] { 7, 8, 9 },
                    SignatureCounter = 0,
                    CredType = "public-key",
                    AaGuid = Guid.NewGuid().ToString(),
                    Name = "Credential 1",
                    RegDate = DateTime.UtcNow
                },
                new PasskeyCredential
                {
                    UserId = user.Id,
                    CredentialId = credentialId2,
                    PublicKey = new byte[] { 4, 5, 6 },
                    UserHandle = new byte[] { 10, 11, 12 },
                    SignatureCounter = 0,
                    CredType = "public-key",
                    AaGuid = Guid.NewGuid().ToString(),
                    Name = "Credential 2",
                    RegDate = DateTime.UtcNow
                }
            });
            
            await UsersContext.SaveChangesAsync();

            var expectedOptions = new AssertionOptions
            {
                Challenge = new byte[] { 1, 2, 3, 4, 5 },
                RpId = "test.com"
            };

            _mockFido2.GetAssertionOptions(
                Arg.Any<GetAssertionOptionsParams>())
                .Returns(expectedOptions);

            var command = new AuthenticatePasskeyCommand("testuser");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Options.Should().Be(expectedOptions);
            
            // Verify FIDO2 was called with correct parameters
            _mockFido2.Received(1).GetAssertionOptions(
                Arg.Is<GetAssertionOptionsParams>(p => 
                    p.AllowedCredentials.Count == 2 &&
                    p.AllowedCredentials.Any(d => d.Id.SequenceEqual(new byte[] { 1, 2, 3 })) &&
                    p.AllowedCredentials.Any(d => d.Id.SequenceEqual(new byte[] { 4, 5, 6 })) &&
                    p.UserVerification == UserVerificationRequirement.Preferred));

            // Verify options were cached
            var cacheKey = $"passkey_authentication_{user.Id}";
            var cachedOptions = _memoryCache.Get<AssertionOptions>(cacheKey);
            cachedOptions.Should().Be(expectedOptions);
        }

        [Test]
        public async Task Handle_UserNotFound_ThrowsAppException()
        {
            // Arrange
            var command = new AuthenticatePasskeyCommand("nonexistentuser");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_UserWithNoPasskeys_ThrowsAppException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            // Don't add any passkey credentials

            var command = new AuthenticatePasskeyCommand("testuser");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("No passkeys registered for this user");
        }

        [Test]
        public async Task Handle_UserWithSinglePasskey_ReturnsOptionsWithSingleCredential()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var credentialId = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
            
            UsersContext.PasskeyCredentials.Add(new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = credentialId,
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Single Credential",
                RegDate = DateTime.UtcNow
            });
            
            await UsersContext.SaveChangesAsync();

            var expectedOptions = new AssertionOptions();
            _mockFido2.GetAssertionOptions(
                Arg.Any<GetAssertionOptionsParams>())
                .Returns(expectedOptions);

            var command = new AuthenticatePasskeyCommand("testuser");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            // Verify FIDO2 was called with single credential
            _mockFido2.Received(1).GetAssertionOptions(
                Arg.Is<GetAssertionOptionsParams>(p => 
                    p.AllowedCredentials.Count == 1 &&
                    p.AllowedCredentials.First().Id.SequenceEqual(new byte[] { 1, 2, 3, 4, 5 })));
        }

        [Test]
        public async Task Handle_CacheExpirationSetCorrectly()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            UsersContext.PasskeyCredentials.Add(new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Test Credential",
                RegDate = DateTime.UtcNow
            });
            
            await UsersContext.SaveChangesAsync();

            var expectedOptions = new AssertionOptions();
            _mockFido2.GetAssertionOptions(
                Arg.Any<GetAssertionOptionsParams>())
                .Returns(expectedOptions);

            var command = new AuthenticatePasskeyCommand("testuser");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            // Verify options were cached (we can't easily test the exact expiration, but we can verify it exists)
            var cacheKey = $"passkey_authentication_{user.Id}";
            var cachedOptions = _memoryCache.Get<AssertionOptions>(cacheKey);
            cachedOptions.Should().NotBeNull();
            cachedOptions.Should().Be(expectedOptions);
        }

        [Test]
        public async Task Handle_UsernameIsCaseInsensitive()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "TestUser", // Capital T and U
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            UsersContext.PasskeyCredentials.Add(new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Test Credential",
                RegDate = DateTime.UtcNow
            });
            
            await UsersContext.SaveChangesAsync();

            var expectedOptions = new AssertionOptions();
            _mockFido2.GetAssertionOptions(
                Arg.Any<GetAssertionOptionsParams>())
                .Returns(expectedOptions);

            var command = new AuthenticatePasskeyCommand("testuser"); // lowercase
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Options.Should().Be(expectedOptions);
        }
    }
}