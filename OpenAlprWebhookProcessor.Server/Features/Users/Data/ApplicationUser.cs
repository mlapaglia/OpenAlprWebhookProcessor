using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Users.Data
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public virtual ICollection<PasskeyCredential> PasskeyCredentials { get; set; } = new List<PasskeyCredential>();
    }
}