using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.DeletePasskey
{
    public class DeletePasskeyCommandHandler : IQueryHandler<DeletePasskeyCommand, DeletePasskeyResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UsersContext _context;

        public DeletePasskeyCommandHandler(
            UserManager<ApplicationUser> userManager,
            UsersContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async ValueTask<DeletePasskeyResponse> Handle(DeletePasskeyCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(request.User);
            if (user == null)
                throw new AppException("User not found");

            var passkey = await _context.PasskeyCredentials
                .FirstOrDefaultAsync(p => p.Id == request.PasskeyId && p.UserId == user.Id, cancellationToken);

            if (passkey == null)
                throw new AppException("Passkey not found");

            _context.PasskeyCredentials.Remove(passkey);
            await _context.SaveChangesAsync(cancellationToken);

            return new DeletePasskeyResponse("Passkey deleted successfully", true);
        }
    }
}
