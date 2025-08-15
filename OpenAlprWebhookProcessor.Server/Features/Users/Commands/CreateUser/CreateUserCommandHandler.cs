using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IQueryHandler<CreateUserCommand, ApplicationUser>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateUserCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<ApplicationUser> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByNameAsync(request.Username);
            if (existingUser != null)
            {
                throw new AppException("Username \"" + request.Username + "\" is already taken");
            }

            var user = new ApplicationUser
            {
                UserName = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Username // Use username as email if no email provided
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new AppException($"Failed to create user: {errors}");
            }

            return user;
        }
    }
}