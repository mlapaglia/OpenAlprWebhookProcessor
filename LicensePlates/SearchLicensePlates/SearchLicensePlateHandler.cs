﻿using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates
{
    public class SearchLicensePlateHandler
    {
        private readonly ProcessorContext _processerContext;

        public SearchLicensePlateHandler(ProcessorContext processorContext)
        {
            _processerContext = processorContext;
        }
        
        public async Task<SearchLicensePlateResponse> HandleAsync(
            SearchLicensePlateRequest request,
            CancellationToken cancellationToken)
        {
            var dbRequest = _processerContext.PlateGroups.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.PlateNumber))
            {
                dbRequest = dbRequest.Where(x => x.Number.Contains(request.PlateNumber));
            }

            if (request.StartSearchOn != null)
            {
                var startEpochMilliseconds = request.StartSearchOn.Value.ToUnixTimeMilliseconds();
                dbRequest = dbRequest.Where(x => x.ReceivedOnEpoch >= startEpochMilliseconds);
            }

            if (request.EndSearchOn != null)
            {
                var endEpochMilliseconds = request.EndSearchOn.Value.ToUnixTimeMilliseconds();
                dbRequest = dbRequest.Where(x => x.ReceivedOnEpoch <= endEpochMilliseconds);
            }

            var platesToIgnore = await GetPlatesToIgnoreAsync(cancellationToken);

            if (!request.FilterIgnoredPlates && platesToIgnore.Count > 0)
            {
                dbRequest = dbRequest.Where(x => !platesToIgnore.Contains(x.Number));
            }

            var totalCount = await dbRequest.CountAsync(cancellationToken);

            dbRequest = dbRequest
                .OrderByDescending(x => x.ReceivedOnEpoch)
                .Skip(request.PageNumber * request.PageSize)
                .Take(request.PageSize);

            var results = await dbRequest.ToListAsync(cancellationToken);

            var licensePlates = new List<LicensePlate>();

            foreach (var plate in results)
            {
                licensePlates.Add(PlateMapper.MapPlate(plate, platesToIgnore));
            }

            return new SearchLicensePlateResponse()
            {
                Plates = licensePlates,
                TotalCount = totalCount,
            };
        }

        private async Task<List<string>> GetPlatesToIgnoreAsync(CancellationToken cancellationToken)
        {
            return (await _processerContext.Ignores.ToListAsync(cancellationToken))
                .Select(x => x.PlateNumber)
                .ToList();
        }
    }
}