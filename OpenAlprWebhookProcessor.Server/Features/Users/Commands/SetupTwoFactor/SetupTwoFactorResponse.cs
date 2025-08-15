namespace OpenAlprWebhookProcessor.Features.Users.Commands.SetupTwoFactor
{
    public class SetupTwoFactorResponse
    {
        public string SharedKey { get; set; } = default!;
        public string QrCodeUri { get; set; } = default!;

        public SetupTwoFactorResponse(string sharedKey, string qrCodeUri)
        {
            SharedKey = sharedKey;
            QrCodeUri = qrCodeUri;
        }
    }
}