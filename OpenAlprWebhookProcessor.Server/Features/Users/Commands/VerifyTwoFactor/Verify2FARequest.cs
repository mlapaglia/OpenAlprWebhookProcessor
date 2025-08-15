namespace OpenAlprWebhookProcessor.Features.Users.Commands.VerifyTwoFactor
{
    public class Verify2FARequest
    {
        public string UserId { get; set; } = default!;
        public string Code { get; set; } = default!;
        public bool RememberMe { get; set; }
    }
}