using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Users.Commands.DeletePasskey;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;
using Tests.TestHelpers;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Tests.Features.Users.Commands
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class DeletePasskeyCommandHandlerTests : TestBase
    {
        private DeletePasskeyCommandHandler _handler;
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
            
            _handler = new DeletePasskeyCommandHandler(_userManager, UsersContext);
        }

        [TearDown]
        public new void TearDown()
        {
            _userManager?.Dispose();
            base.TearDown();
        }

        [Test]
        public async Task Handle_ValidPasskeyDeletion_ReturnsSuccessResponse()
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

            var passkey = new PasskeyCredential
            {
                UserId = user.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }),
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Test Passkey",
                RegDate = DateTime.UtcNow
            };
            
            UsersContext.PasskeyCredentials.Add(passkey);
            await UsersContext.SaveChangesAsync();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var command = new DeletePasskeyCommand(userClaims, passkey.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Passkey deleted successfully");
            
            // Verify passkey was removed from database
            var deletedPasskey = UsersContext.PasskeyCredentials.FirstOrDefault(p => p.Id == passkey.Id);
            deletedPasskey.Should().BeNull();
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

            var command = new DeletePasskeyCommand(userClaims, 123);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("User not found");
        }

        [Test]
        public async Task Handle_PasskeyNotFound_ThrowsAppException()
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

            var command = new DeletePasskeyCommand(userClaims, 999); // Non-existent passkey ID
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Passkey not found");
        }

        [Test]
        public async Task Handle_PasskeyBelongsToAnotherUser_ThrowsAppException()
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

            // Create passkey for user2
            var passkey = new PasskeyCredential
            {
                UserId = user2.Id,
                CredentialId = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }),
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "User2 Passkey",
                RegDate = DateTime.UtcNow
            };
            
            UsersContext.PasskeyCredentials.Add(passkey);
            await UsersContext.SaveChangesAsync();

            // Try to delete user2's passkey as user1
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user1.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user1.Id.ToString())
            }));

            var command = new DeletePasskeyCommand(userClaims, passkey.Id);
            var cancellationToken = GetCancellationToken();

            // Act & Assert
            await FluentActions.Invoking(async () => await _handler.Handle(command, cancellationToken))
                .Should().ThrowExactlyAsync<AppException>()
                .WithMessage("Passkey not found");
                
            // Verify passkey still exists
            var existingPasskey = UsersContext.PasskeyCredentials.FirstOrDefault(p => p.Id == passkey.Id);
            existingPasskey.Should().NotBeNull();
        }

        [Test]
        public async Task Handle_UserWithMultiplePasskeys_DeletesOnlySpecifiedPasskey()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com"
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
                Name = "Passkey 1",
                RegDate = DateTime.UtcNow
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
                Name = "Passkey 2",
                RegDate = DateTime.UtcNow
            };
            
            UsersContext.PasskeyCredentials.AddRange(passkey1, passkey2);
            await UsersContext.SaveChangesAsync();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var command = new DeletePasskeyCommand(userClaims, passkey1.Id);
            var cancellationToken = GetCancellationToken();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            
            // Verify only passkey1 was deleted
            var deletedPasskey = UsersContext.PasskeyCredentials.FirstOrDefault(p => p.Id == passkey1.Id);
            deletedPasskey.Should().BeNull();
            
            var remainingPasskey = UsersContext.PasskeyCredentials.FirstOrDefault(p => p.Id == passkey2.Id);
            remainingPasskey.Should().NotBeNull();
            remainingPasskey!.Name.Should().Be("Passkey 2");
        }

        [Test]
        public async Task Handle_DatabaseSaveChanges_IsCalled()
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
                CredentialId = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }),
                PublicKey = new byte[] { 1, 2, 3 },
                UserHandle = new byte[] { 4, 5, 6 },
                SignatureCounter = 0,
                CredType = "public-key",
                AaGuid = Guid.NewGuid().ToString(),
                Name = "Test Passkey",
                RegDate = DateTime.UtcNow
            };
            
            UsersContext.PasskeyCredentials.Add(passkey);
            await UsersContext.SaveChangesAsync();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            var command = new DeletePasskeyCommand(userClaims, passkey.Id);
            var cancellationToken = GetCancellationToken();

            var initialCount = UsersContext.PasskeyCredentials.Count();

            // Act
            var result = await _handler.Handle(command, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            
            // Verify database was updated (count decreased by 1)
            var finalCount = UsersContext.PasskeyCredentials.Count();
            finalCount.Should().Be(initialCount - 1);
        }
    }
}