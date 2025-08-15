namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetTwoFactorStatus
{
    public class TwoFactorStatusResponse
    {
        public bool IsTwoFactorEnabled { get; set; }
        public bool HasAuthenticator { get; set; }

        public TwoFactorStatusResponse(bool isTwoFactorEnabled, bool hasAuthenticator)
        {
            IsTwoFactorEnabled = isTwoFactorEnabled;
            HasAuthenticator = hasAuthenticator;
        }
    }
}