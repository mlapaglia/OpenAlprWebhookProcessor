namespace OpenAlprWebhookProcessor.Features.Users.Commands.EnableTwoFactor
{
    public class EnableTwoFactorRequest
    {
        public string Code { get; set; } = default!;
    }
}