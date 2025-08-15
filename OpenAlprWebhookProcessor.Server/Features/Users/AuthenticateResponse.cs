using OpenAlprWebhookProcessor.Features.Users.Data;

namespace OpenAlprWebhookProcessor.Features.Users
{
    public class AuthenticateResponse
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Username { get; set; }

        public bool Success { get; set; } = true;

        public bool RequiresTwoFactor { get; set; }

        public string UserId { get; set; }

        public string Message { get; set; }

        public AuthenticateResponse()
        {
        }

        public AuthenticateResponse(ApplicationUser user)
        {
            Id = user.Id.ToString();
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.UserName;
            Success = true;
        }
    }
}
