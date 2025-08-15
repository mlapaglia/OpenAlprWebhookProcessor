using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<List<UserDto>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default)
        {
            var users = await _userManager.Users.ToListAsync(cancellationToken);
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TwoFactorEnabled = twoFactorEnabled
                });
            }

            return userDtos;
        }
    }
}