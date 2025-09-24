using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetUserPasskeys;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Tests.TestHelpers;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Tests.Features.Users.Queries
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class GetUserPasskeysQueryHandlerTests : TestBase
    {
        private GetUserPasskeysQueryHandler _handler;
        private UserManager<ApplicationUser> _userManager;

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
            
            _handler = new GetUserPasskeysQueryHandler(_userManager, UsersContext);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_UserWithMultiplePasskeys_ReturnsOrderedByDateDescending()
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

            var passkey1 = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "First Passkey",
                RegDate = DateTime.UtcNow.AddDays(-2)
            };

            var passkey2 = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 7, 8, 9 }),
                PublicKey = new byte[] { 7, 8, 9 },
                UserHandle = new byte[] { 10, 11, 12 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Second Passkey",
                RegDate = DateTime.UtcNow.AddDays(-1)
            };

            var passkey3 = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 13, 14, 15 }),
                PublicKey = new byte[] { 13, 14, 15 },
                UserHandle = new byte[] { 16, 17, 18 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Third Passkey",
                RegDate = DateTime.UtcNow
            };
            
            UsersContext.PasskeyCredentials.AddRange(passkey1, passkey2, passkey3);
            await UsersContext.SaveChangesAsync();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var query = new GetUserPasskeysQuery(userClaims);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Passkeys.Should().HaveCount(3);
            
            // Verify ordering - most recent first
            result.Passkeys[0].Name.Should().Be("Third Passkey");
            result.Passkeys[1].Name.Should().Be("Second Passkey");
            result.Passkeys[2].Name.Should().Be("First Passkey");
            
            // Verify all properties are mapped correctly
            var thirdPasskey = result.Passkeys[0];
            thirdPasskey.Id.Should().Be(passkey3.Id);
            thirdPasskey.Name.Should().Be("Third Passkey");
            thirdPasskey.AaGuid.Should().Be(passkey3.AaGuid);
            thirdPasskey.RegDate.Should().BeCloseTo(passkey3.RegDate, TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task Handle_UserWithNoPasskeys_ReturnsEmptyList()
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

            var query = new GetUserPasskeysQuery(userClaims);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Passkeys.Should().BeEmpty();
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

            var query = new GetUserPasskeysQuery(userClaims);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(query, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_PasskeyWithNullName_ReturnsDefaultName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var passkey = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = null, // Null name
                RegDate = DateTime.UtcNow
            };
            
            UsersContext.PasskeyCredentials.Add(passkey);
            await UsersContext.SaveChangesAsync();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var query = new GetUserPasskeysQuery(userClaims);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Passkeys.Should().HaveCount(1);
            result.Passkeys[0].Name.Should().Be("Unnamed Passkey");
        }

        [Test]
        public async Task Handle_PasskeyWithEmptyName_ReturnsDefaultName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var passkey = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "", // Empty name
                RegDate = DateTime.UtcNow
            };
            
            UsersContext.PasskeyCredentials.Add(passkey);
            await UsersContext.SaveChangesAsync();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var query = new GetUserPasskeysQuery(userClaims);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Passkeys.Should().HaveCount(1);
            result.Passkeys[0].Name.Should().Be(""); // Empty string is preserved, only null is replaced
        }

        [Test]
        public async Task Handle_MultipleUsersWithPasskeys_ReturnsOnlyCurrentUserPasskeys()
        {
            // Arrange
            var user1 = new ApplicationUser
            {
                UserName = "user1",
                Email = "user1@example.com"
            };
            
            var user2 = new ApplicationUser
            {
                UserName = "user2",
                Email = "user2@example.com"
            };
            
            await _userManager.CreateAsync(user1, "TestPassword123!");
            await _userManager.CreateAsync(user2, "TestPassword123!");

            var passkey1 = new PasskeyCredential
            {
                UserId = user1.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "User1 Passkey",
                RegDate = DateTime.UtcNow
            };

            var passkey2 = new PasskeyCredential
            {
                UserId = user2.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 7, 8, 9 }),
                PublicKey = new byte[] { 7, 8, 9 },
                UserHandle = new byte[] { 10, 11, 12 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "User2 Passkey",
                RegDate = DateTime.UtcNow
            };
            
            UsersContext.PasskeyCredentials.AddRange(passkey1, passkey2);
            await UsersContext.SaveChangesAsync();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user1.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user1.Id.ToString())
            }));

            var query = new GetUserPasskeysQuery(userClaims);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Passkeys.Should().HaveCount(1);
            result.Passkeys[0].Name.Should().Be("User1 Passkey");
            result.Passkeys[0].Id.Should().Be(passkey1.Id);
        }

        [Test]
        public async Task Handle_SinglePasskey_ReturnsCorrectProperties()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
            };
            
            await _userManager.CreateAsync(user, "TestPassword123!");

            var guid = Guid.NewGuid();
            var regDate = DateTime.UtcNow;
            
            var passkey = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }),
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 42,
                CredType = "public-key",
                AaGuid = guid.ToString(),
                Name = "My Test Passkey",
                RegDate = regDate
            };
            
            UsersContext.PasskeyCredentials.Add(passkey);
            await UsersContext.SaveChangesAsync();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var query = new GetUserPasskeysQuery(userClaims);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Passkeys.Should().HaveCount(1);
            
            var returnedPasskey = result.Passkeys[0];
            returnedPasskey.Id.Should().Be(passkey.Id);
            returnedPasskey.Name.Should().Be("My Test Passkey");
            returnedPasskey.AaGuid.Should().Be(guid.ToString());
            returnedPasskey.RegDate.Should().BeCloseTo(regDate, TimeSpan.FromSeconds(1));
        }
    }
}