using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.UpdatedCameras
{
    public class UpsertIgnoresRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertIgnoresRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task AddIgnoreAsync(Ignore ignore)
        {
            var dbIgnores = await _processorContext.Ignores
                .Where(x => x.PlateNumber == ignore.PlateNumber)
                .ToListAsync();

            if (dbIgnores != null)
            {
                var addedIgnore = new Data.Ignore()
                {
                    Description = ignore.Description,
                    IsStrictMatch = ignore.StrictMatch,
                    PlateNumber = ignore.PlateNumber,
                };

                _processorContext.Ignores.Add(addedIgnore);

                await _processorContext.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("ignore already exists");
            }
        }

        public async Task UpsertIgnoresAsync(List<Ignore> ignores)
        {
            var dbIgnores = await _processorContext.Ignores.ToListAsync();

            var ignoresToRemove = dbIgnores.Where(p => !ignores.Any(p2 => p2.Id == p.Id));

            _processorContext.RemoveRange(ignoresToRemove);

            var ignoresToUpdate = dbIgnores.Where(x => ignores.Any(p2 => p2.Id == x.Id));

            foreach (var ignoreToUpdate in ignoresToUpdate)
            {
                var updatedIgnore = ignores.First(x => x.Id == ignoreToUpdate.Id);

                ignoreToUpdate.Description = updatedIgnore.Description;
                ignoreToUpdate.IsStrictMatch = updatedIgnore.StrictMatch;
                ignoreToUpdate.PlateNumber = updatedIgnore.PlateNumber;
            }

            var ignoresToAdd = ignores.Where(x => !dbIgnores.Any(p2 => p2.Id == x.Id));

            foreach (var ignoreToAdd in ignoresToAdd)
            {
                var addedIgnore = new Data.Ignore()
                {
                    Description = ignoreToAdd.Description,
                    IsStrictMatch = ignoreToAdd.StrictMatch,
                    PlateNumber = ignoreToAdd.PlateNumber,
                };

                _processorContext.Ignores.Add(addedIgnore);
            }

            await _processorContext.SaveChangesAsync();
        }
    }
}
