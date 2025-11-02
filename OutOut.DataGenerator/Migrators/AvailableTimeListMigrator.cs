using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Persistence.Data;

namespace OutOut.DataGenerator.Migrators
{
    public class AvailableTimeListMigrator
    {
        private readonly ApplicationNonSqlDbContext dbContext;

        public AvailableTimeListMigrator(ApplicationNonSqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task MigrateAvailableTimesList()
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
                    var updatedDif = Builders<Venue>.Update.Set(v => v.Offers[-1].ValidOn, new List<AvailableTime> {  new AvailableTime
                    {
                    Days = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday },
                    From = new TimeSpan(18, 0, 0),
                    To = new TimeSpan(23, 59, 59)
                    },
                    new AvailableTime
                    {
                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday },
                    From = new TimeSpan(0, 0, 0),
                    To = new TimeSpan(3, 0, 0)
                    } });
                    await collection.UpdateOneAsync(filter, updatedDif);
                }
            }
        }
    }
}
