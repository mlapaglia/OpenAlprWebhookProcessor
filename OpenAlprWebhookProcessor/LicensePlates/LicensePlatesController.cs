using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.LicensePlates.GetLicensePlateCounts;
using OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates
{
    [Authorize]
    [ApiController]
    [Route("licensePlates")]
    public class LicensePlatesController : ControllerBase
    {
        private readonly SearchLicensePlateHandler _searchLicensePlateHandler;

        private readonly GetLicensePlateCountsHandler _getLicensePlateCountsHandler;

        public LicensePlatesController(
            SearchLicensePlateHandler searchLicensePlateHandler,
            GetLicensePlateCountsHandler getLicensePlateCountsHandler)
        {
            _searchLicensePlateHandler = searchLicensePlateHandler;
            _getLicensePlateCountsHandler = getLicensePlateCountsHandler;
        }

        [HttpPost("search")]
        public async Task<SearchLicensePlateResponse> SearchPlates(
            [FromBody] SearchLicensePlateRequest request,
            CancellationToken cancellationToken)
        {
            return await _searchLicensePlateHandler.HandleAsync(
                request,
                cancellationToken);
        }
        
        [HttpGet("counts")]
        public async Task<GetLicensePlateCountsResponse> GetLicensePlateCounts(CancellationToken cancellationToken)
        {
            var request = new GetLicensePlateCountsRequest();

            return await _getLicensePlateCountsHandler.HandleAsync(
                request,
                cancellationToken);
        }
    }
}
