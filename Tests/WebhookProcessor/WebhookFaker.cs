using Bogus;
using OpenAlprWebhookProcessor.Server.WebhookProcessor.OpenAlprWebhook;

namespace Tests.WebhookProcessor
{
    public static class WebhookFaker
    {
        private static readonly List<string> _fakeDataTypes = new List<string>()
        {
            "alpr_alert"
        };

        private static readonly List<string> _fakeListTypes = new List<string>()
        {
            "Whitelist",
            "Blacklist",
            "Watchlist"
        };

        public static IEnumerable<Webhook> Generate()
        {
            var webhookFaker = new Faker<Webhook>()
                .RuleFor(w => w.DataType, f => f.PickRandom(_fakeDataTypes))
                .RuleFor(w => w.Version, f => f.Random.Int(1, 3))
                .RuleFor(w => w.EpochTime, f => DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                .RuleFor(w => w.AgentUid, f => f.Random.Guid().ToString())
                .RuleFor(w => w.AlertList, f => f.Random.Word())
                .RuleFor(w => w.SiteName, f => f.Company.CompanyName())
                .RuleFor(w => w.CameraName, f => $"Cam{f.Random.Int(1, 10)}")
                .RuleFor(w => w.CameraNumber, f => f.Random.Int(1, 10))
                .RuleFor(w => w.PlateNumber, f => f.Vehicle.Vin().Substring(0, 7))
                .RuleFor(w => w.Description, f => f.Lorem.Sentence())
                .RuleFor(w => w.ListType, f => f.PickRandom(_fakeListTypes))
                .RuleFor(w => w.Group, f => new Group { /* setup Group properties similarly */ });

            return webhookFaker.GenerateForever();
        }
    }
}
