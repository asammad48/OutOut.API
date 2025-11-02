using OutOut.Models.Models;
using OutOut.Persistence.Data;
using MongoDB.Driver;
using MongoDB.Bson;

namespace OutOut.DataGenerator.Editors
{
    public class EventsEditor
    {
        private readonly ApplicationNonSqlDbContext dbContext;

        public EventsEditor(ApplicationNonSqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task ModifyPackages()
        {
            var collection = dbContext.GetCollection<Event>();

            var entries = (await collection.FindAsync(entity => true)).ToList();
            var packages = new List<EventPackage>
            {
                new EventPackage { Id = ObjectId.GenerateNewId().ToString(), Title = "Soft Drinks Package", Price = 299, TicketsNumber = 50, RemainingTickets = 50},
                new EventPackage { Id = ObjectId.GenerateNewId().ToString(), Title = "House Package", Price = 399, TicketsNumber = 150, RemainingTickets = 150}
            };
            foreach (var entry in entries)
            {
                foreach (var occurrence in entry.Occurrences)
                {
                    var updatedDif = Builders<Event>.Update.Set(a => a.Occurrences[-1].Packages, packages);
                    var filter = Builders<Event>.Filter.ElemMatch(v => v.Occurrences, v => v.Id == occurrence.Id);
                    await collection.UpdateOneAsync(filter, updatedDif);
                }
            }
        }
    }
}
