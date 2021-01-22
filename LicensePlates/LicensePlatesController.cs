using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public LicensePlatesController(SearchLicensePlateHandler searchLicensePlateHandler)
        {
            _searchLicensePlateHandler = searchLicensePlateHandler;
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
    }
}
