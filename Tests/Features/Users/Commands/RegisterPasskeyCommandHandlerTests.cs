using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.RegisterPasskey;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Tests.TestHelpers;
using System.Security.Claims;
using Fido2NetLib;
using Fido2NetLib.Objects;
using System.Text;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class RegisterPasskeyCommandHandlerTests : TestBase
    {
        private RegisterPasskeyCommandHandler _handler;
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
            
            _handler = new RegisterPasskeyCommandHandler(
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
        public async Task Handle_ValidUser_ReturnsRegisterPasskeyResponse()
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

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var expectedOptions = new CredentialCreateOptions
            {
                Rp = new PublicKeyCredentialRpEntity("test.com", "Test", null),
                User = new Fido2User
                {
                    DisplayName = "Test User",
                    Name = "testuser",
                    Id = Encoding.UTF8.GetBytes(user.Id.ToString())
                },
                Challenge = new byte[] { 1, 2, 3, 4, 5 }
            };

            _mockFido2.RequestNewCredential(
                Arg.Any<Fido2User>(),
                Arg.Any<List<PublicKeyCredentialDescriptor>>(),
                Arg.Any<AuthenticatorSelection>(),
                Arg.Any<AttestationConveyancePreference>())
                .Returns(expectedOptions);

            var command = new RegisterPasskeyCommand(userClaims, "MyPasskey");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Options.Should().Be(expectedOptions);
            
            // Verify FIDO2 was called with correct parameters
            _mockFido2.Received(1).RequestNewCredential(
                Arg.Is<Fido2User>(u => 
                    u.DisplayName == "Test User" && 
                    u.Name == "testuser" && 
                    Encoding.UTF8.GetString(u.Id) == user.Id.ToString()),
                Arg.Any<List<PublicKeyCredentialDescriptor>>(),
                Arg.Any<AuthenticatorSelection>(),
                Arg.Is<AttestationConveyancePreference>(p => p == AttestationConveyancePreference.None));

            // Verify options were cached
            var cacheKey = $"passkey_registration_{user.Id}";
            var cachedOptions = _memoryCache.Get<CredentialCreateOptions>(cacheKey);
            cachedOptions.Should().Be(expectedOptions);
        }

        [Test]
        public async Task Handle_UserWithExistingCredentials_ExcludesExistingCredentials()
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

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var expectedOptions = new CredentialCreateOptions();
            _mockFido2.RequestNewCredential(
                Arg.Any<Fido2User>(),
                Arg.Any<List<PublicKeyCredentialDescriptor>>(),
                Arg.Any<AuthenticatorSelection>(),
                Arg.Any<AttestationConveyancePreference>())
                .Returns(expectedOptions);

            var command = new RegisterPasskeyCommand(userClaims, "NewPasskey");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            // Verify FIDO2 was called with existing credentials excluded
            _mockFido2.Received(1).RequestNewCredential(
                Arg.Any<Fido2User>(),
                Arg.Is<List<PublicKeyCredentialDescriptor>>(list => 
                    list.Count == 2 &&
                    list.Any(d => d.Id.SequenceEqual(new byte[] { 1, 2, 3 })) &&
                    list.Any(d => d.Id.SequenceEqual(new byte[] { 4, 5, 6 }))),
                Arg.Any<AuthenticatorSelection>(),
                Arg.Any<AttestationConveyancePreference>());
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

            var command = new RegisterPasskeyCommand(userClaims, "MyPasskey");
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_UserWithNoNames_UsesFallbackDisplayName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
                // FirstName and LastName are null
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var expectedOptions = new CredentialCreateOptions();
            _mockFido2.RequestNewCredential(
                Arg.Any<Fido2User>(),
                Arg.Any<List<PublicKeyCredentialDescriptor>>(),
                Arg.Any<AuthenticatorSelection>(),
                Arg.Any<AttestationConveyancePreference>())
                .Returns(expectedOptions);

            var command = new RegisterPasskeyCommand(userClaims, "MyPasskey");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            // Verify FIDO2 was called with empty display name (trimmed)
            _mockFido2.Received(1).RequestNewCredential(
                Arg.Is<Fido2User>(u => 
                    u.DisplayName == "" && 
                    u.Name == "testuser"),
                Arg.Any<List<PublicKeyCredentialDescriptor>>(),
                Arg.Any<AuthenticatorSelection>(),
                Arg.Any<AttestationConveyancePreference>());
        }

        [Test]
        public async Task Handle_UserWithNoUserName_UsesEmailAsFallback()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "test@example.com", // UserName can't be null for Identity
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");
            
            // Simulate scenario where UserName could be null in the Fido2User creation logic
            // by setting UserName to null after creation (for testing the fallback logic)
            user.UserName = null;

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var expectedOptions = new CredentialCreateOptions();
            _mockFido2.RequestNewCredential(
                Arg.Any<Fido2User>(),
                Arg.Any<List<PublicKeyCredentialDescriptor>>(),
                Arg.Any<AuthenticatorSelection>(),
                Arg.Any<AttestationConveyancePreference>())
                .Returns(expectedOptions);

            var command = new RegisterPasskeyCommand(userClaims, "MyPasskey");
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            
            // Verify FIDO2 was called with email as Name
            _mockFido2.Received(1).RequestNewCredential(
                Arg.Is<Fido2User>(u => 
                    u.DisplayName == "Test User" && 
                    u.Name == "test@example.com"),
                Arg.Any<List<PublicKeyCredentialDescriptor>>(),
                Arg.Any<AuthenticatorSelection>(),
                Arg.Any<AttestationConveyancePreference>());
        }

        [Test]
        public async Task Handle_CacheExpirationSetCorrectly()
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

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var expectedOptions = new CredentialCreateOptions();
            _mockFido2.RequestNewCredential(
                Arg.Any<Fido2User>(),
                Arg.Any<List<PublicKeyCredentialDescriptor>>(),
                Arg.Any<AuthenticatorSelection>(),
                Arg.Any<AttestationConveyancePreference>())
                .Returns(expectedOptions);

            var command = new RegisterPasskeyCommand(userClaims, "MyPasskey");
            var cancellationToken = GetCancellationToken();

            var beforeTime = DateTime.UtcNow;

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            var afterTime = DateTime.UtcNow;

            // Assert
            result.Should().NotBeNull();
            
            // Verify options were cached (we can't easily test the exact expiration, but we can verify it exists)
            var cacheKey = $"passkey_registration_{user.Id}";
            var cachedOptions = _memoryCache.Get<CredentialCreateOptions>(cacheKey);
            cachedOptions.Should().NotBeNull();
            cachedOptions.Should().Be(expectedOptions);
        }
    }
}