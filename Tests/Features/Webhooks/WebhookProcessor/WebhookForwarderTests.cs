using AwesomeAssertions;
using Flurl.Http;
using Flurl.Http.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OpenAlprWebhookProcessor.Features.Webhooks.WebhookProcessor;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests.Features.Webhooks.WebhookProcessor
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class WebhookForwarderTests : TestBase
    {
        private WebhookForwarder _forwarder;
        private IFlurlClientCache _mockFlurlClientCache;
        private IFlurlClient _mockFlurlClient;
        private IFlurlRequest _mockFlurlRequest;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockFlurlClientCache = Substitute.For<IFlurlClientCache>();
            _mockFlurlClient = Substitute.For<IFlurlClient>();
            _mockFlurlRequest = Substitute.For<IFlurlRequest>();

            _mockFlurlClient.Request(Arg.Any<string>()).Returns(_mockFlurlRequest);
            _mockFlurlClientCache.GetOrAdd(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Action<IFlurlClientBuilder>>())
                .Returns(_mockFlurlClient);

            _forwarder = new WebhookForwarder(_mockFlurlClientCache);
        }

        [TearDown]
        public override void TearDown()
        {
            _mockFlurlClient?.Dispose();
            base.TearDown();
        }














    }
}
