using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate
{
    public class AuthenticateCommandHandler : IQueryHandler<AuthenticateCommand, UserDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthenticateCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async ValueTask<UserDto> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                throw new AppException("Username or password is incorrect");

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                true);

            if (result.IsLockedOut)
                throw new AppException("Account locked due to multiple failed attempts");

            if (!result.Succeeded)
                throw new AppException("Username or password is incorrect");

            var authUser = new UserDto
            {
                FirstName = user.FirstName,
                Id = user.Id,
                TwoFactorEnabled = false,
                LastName = user.LastName,
                Username = user.UserName,
            };

            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                authUser.TwoFactorEnabled = true;
                return authUser;
            }

            await _signInManager.SignInAsync(user, request.RememberMe);

            return authUser;
        }
    }
}