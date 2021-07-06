using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.Tags
{
    public class UpsertTagsRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertTagsRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task HandleAsync(List<Tag> tags)
        {
            tags = tags.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();

            var dbtags = await _processorContext.Tags.ToListAsync();

            var tagsToRemove = dbtags.Where(p => !tags.Any(p2 => p2.Id == p.Id));

            _processorContext.RemoveRange(tagsToRemove);

            var tagsToUpdate = dbtags.Where(x => tags.Any(p2 => p2.Id == x.Id));

            foreach (var tagToUpdate in tagsToUpdate)
            {
                var updatedTag = tags.First(x => x.Id == tagToUpdate.Id);

                tagToUpdate.Name = updatedTag.Name;
            }

            var tagsToAdd = tags.Where(x => !dbtags.Any(p2 => p2.Id == x.Id));

            foreach (var tagToAdd in tagsToAdd)
            {
                var addedTag = new Data.Tag()
                {
                    Name = tagToAdd.Name,
                };

                _processorContext.Tags.Add(addedTag);
            }

            await _processorContext.SaveChangesAsync();
        }
    }
}
