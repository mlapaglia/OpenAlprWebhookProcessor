using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.LicensePlates.GetLicensePlate;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates
{
    [Route("licensePlates")]
    public class LicensePlatesController : ControllerBase
    {
        private readonly bool _webRequestLoggingEnabled;

        private readonly ILogger<LicensePlatesController> _logger;

        private readonly GetLicensePlateHandler _getLicensePlateHandler;

        public LicensePlatesController(
            IConfiguration configuration,
            ILogger<LicensePlatesController> logger,
            GetLicensePlateHandler getLicensePlateHandler)
        {
            _webRequestLoggingEnabled = configuration.GetValue("WebRequestLoggingEnabled", false);
            _logger = logger;
            _getLicensePlateHandler = getLicensePlateHandler;
        }

        [HttpPost]
        [Route("{licensePlate}")]
        public async Task<List<LicensePlate>> Get(
            string licensePlate,
            CancellationToken cancellationToken)
        {
            if (_webRequestLoggingEnabled)
            {
                _logger.LogInformation("request received {0}", licensePlate);
            }

            return await _getLicensePlateHandler.GetLicensePlatesAsync(
                licensePlate,
                cancellationToken);
        }
    }
}
