using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.LicensePlates.DeletePlate;
using OpenAlprWebhookProcessor.LicensePlates.GetLicensePlateCounts;
using OpenAlprWebhookProcessor.LicensePlates.GetPlateFilters;
using OpenAlprWebhookProcessor.LicensePlates.GetStatistics;
using OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates;
using OpenAlprWebhookProcessor.LicensePlates.UpsertPlate;
using System;
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

        private readonly DeleteLicensePlateGroupRequestHandler _deleteLicensePlateGroupHandler;

        private readonly GetLicensePlateFiltersHandler _getLicensePlateFiltersHandler;

        private readonly GetStatisticsHandler _getStatisticsHandler;

        private readonly UpsertPlateRequestHandler _upsertPlateRequestHandler;

        public LicensePlatesController(
            SearchLicensePlateHandler searchLicensePlateHandler,
            GetLicensePlateCountsHandler getLicensePlateCountsHandler,
            DeleteLicensePlateGroupRequestHandler deleteLicensePlateGroupHandler,
            GetLicensePlateFiltersHandler getLicensePlateFiltersHandler,
            GetStatisticsHandler getStatisticsHandler,
            UpsertPlateRequestHandler upsertPlateRequestHandler)
        {
            _searchLicensePlateHandler = searchLicensePlateHandler;
            _getLicensePlateCountsHandler = getLicensePlateCountsHandler;
            _deleteLicensePlateGroupHandler = deleteLicensePlateGroupHandler;
            _getLicensePlateFiltersHandler = getLicensePlateFiltersHandler;
            _getStatisticsHandler = getStatisticsHandler;
            _upsertPlateRequestHandler = upsertPlateRequestHandler;
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

        [HttpPost("edit")]
        public async Task UpsertPlate(
            [FromBody] LicensePlate licensePlate,
            CancellationToken cancellationToken)
        {
            await _upsertPlateRequestHandler.HandleAsync(
                licensePlate,
                cancellationToken);
        }

        [HttpDelete("{plateId}")]
        public async Task DeletePlate(
            Guid plateId,
            CancellationToken cancellationToken)
        {
            await _deleteLicensePlateGroupHandler.HandleAsync(
                plateId,
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

        [HttpGet("filters")]
        public async Task<GetLicensePlateFiltersResponse> GetLicensePlateFilters(CancellationToken cancellationToken)
        {
            return await _getLicensePlateFiltersHandler.HandleAsync(cancellationToken);
        }

        [HttpGet("statistics/{plateNumber}")]
        public async Task<PlateStatistics> GetPlateStatistics(
            string plateNumber,
            CancellationToken cancellationToken)
        {
            return await _getStatisticsHandler.HandleAsync(
                plateNumber,
                cancellationToken);
        }
    }
}
