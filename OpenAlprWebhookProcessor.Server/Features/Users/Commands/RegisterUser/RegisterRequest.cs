namespace OpenAlprWebhookProcessor.Features.Users.Commands.RegisterUser
{
    public class RegisterRequest
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
    }
}