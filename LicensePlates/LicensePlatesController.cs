using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenAlprWebhookProcessor.LicensePlates.GetLicensePlate;
using OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates;
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
        private readonly GetLicensePlateHandler _getLicensePlateHandler;

        private readonly SearchLicensePlateHandler _searchLicensePlateHandler;

        public LicensePlatesController(
            GetLicensePlateHandler getLicensePlateHandler,
            SearchLicensePlateHandler searchLicensePlateHandler)
        {
            _getLicensePlateHandler = getLicensePlateHandler;
            _searchLicensePlateHandler = searchLicensePlateHandler;
        }

        [HttpGet("{licensePlate}")]
        public async Task<List<LicensePlate>> Get(
            string licensePlate,
            CancellationToken cancellationToken)
        {
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

        [HttpGet("search")]
        public async Task<GetLicensePlateResponse> SearchPlates(
            [FromBody] SearchLicensePlateRequest request,
            CancellationToken cancellationToken)
        {
            return await _searchLicensePlateHandler.HandleAsync(
                request,
                cancellationToken);
        }
    }
}
