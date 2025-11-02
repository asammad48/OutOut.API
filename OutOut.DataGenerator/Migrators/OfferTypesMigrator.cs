using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Persistence.Data;

namespace OutOut.DataGenerator.Migrators
{
    public class OfferTypesMigrator
    {
        private readonly ApplicationNonSqlDbContext dbContext;

        public OfferTypesMigrator(ApplicationNonSqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task MigrateVenues()
        {
            var collection = dbContext.GetCollection<Venue>();
            var offerTypeCollection = dbContext.GetCollection<OfferType>();

            var entries = (await collection.FindAsync(entity => true)).ToList();
            var offerTypes = (await offerTypeCollection.FindAsync(entity => true)).ToList();

            foreach (var entry in entries)
            {
                foreach (var offer in entry.Offers)
                {
                    var filter = Builders<Venue>.Filter.ElemMatch(a => a.Offers, a => a.Id == offer.Id);
                    var updatedDif = Builders<Venue>.Update.Set(v => v.Offers[-1].Type, GenerateRandom(offerTypes));
                    await collection.UpdateOneAsync(filter, updatedDif);
                }
            }
        }
        public T GenerateRandom<T>(List<T> list)
        {
            Random random = new Random();
            int r = random.Next(list.Count);
            return list[r];
        }
    }
}
