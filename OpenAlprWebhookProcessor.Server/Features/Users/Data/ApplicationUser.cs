using Microsoft.AspNetCore.Identity;

namespace OpenAlprWebhookProcessor.Features.Users.Data
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}