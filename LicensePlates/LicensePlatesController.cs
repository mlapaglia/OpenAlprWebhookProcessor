
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.LicensePlates.GetLicensePlate;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates
{
    [Authorize]
    [ApiController]
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

        [HttpGet("{licensePlate}")]
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

        [HttpGet("recent")]
        public async Task<GetLicensePlateResponse> GetRecent(
            CancellationToken cancellationToken,
            int pageSize = 10,
            int pageNumber = 0)
        {
            if (_webRequestLoggingEnabled)
            {
                _logger.LogInformation("recent plates request received num: {0} page {1}", pageSize, pageNumber);
            }

            var plates = await _getLicensePlateHandler.GetRecentPlatesAsync(
                pageNumber,
                pageSize,
                cancellationToken);

            var totalCount = await _getLicensePlateHandler.GetTotalNumberOfPlatesAsync(cancellationToken);

            return new GetLicensePlateResponse()
            {
                Plates = plates,
                TotalCount = totalCount,
            };
        }
    }
}
