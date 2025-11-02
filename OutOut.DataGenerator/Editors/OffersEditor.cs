using OutOut.Models.Models;
using OutOut.Persistence.Data;
using MongoDB.Driver;

namespace OutOut.DataGenerator.Editors
{
    public class OffersEditor
    {
        private readonly ApplicationNonSqlDbContext dbContext;

        public OffersEditor(ApplicationNonSqlDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task ModifyOffers()
        {
            var collection = dbContext.GetCollection<Venue>();

            var entries = (await collection.FindAsync(entity => true)).ToList();
            foreach (var entry in entries)
            {
                foreach (var offer in entry.Offers)
                {
                    var updateAssignedDateDif = Builders<Venue>.Update.Set(a => a.Offers[-1].AssignDate, DateTime.UtcNow);
                    var filter = Builders<Venue>.Filter.ElemMatch(v => v.Offers, v => v.Id == offer.Id);
                    await collection.UpdateOneAsync(filter, updateAssignedDateDif);
                }
            }
        }
    }
}
