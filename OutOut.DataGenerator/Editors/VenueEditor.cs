using OutOut.Models.Models;
using OutOut.Persistence.Data;
using MongoDB.Driver;

namespace OutOut.DataGenerator.Editors
{
    public class VenueEditor
    {
        private readonly ApplicationNonSqlDbContext dbContext;
        private readonly Random random;

        public VenueEditor(ApplicationNonSqlDbContext dbContext)
        {
            this.dbContext = dbContext;
            random = new Random();
        }

        public async Task ModifyDescription()
        {
            var collection = dbContext.GetCollection<Venue>();

            var entries = (await collection.FindAsync(entity => true)).ToList();

            foreach (var entry in entries)
            {
                var updatedDif = Builders<Venue>.Update.Set(v => v.Description, GenerateFrom(entry.Description));
                var filter = Builders<Venue>.Filter.Eq(v => v.Id, entry.Id);
                await collection.UpdateOneAsync(filter, updatedDif);
            }
        }

        private string GenerateFrom(string currentString)
        {
            int from = random.Next(1, currentString.Length/4);
            int to = random.Next(from, currentString.Length);
            int length = to - from + 1;

            var newString = currentString.Substring(from, length);

            return $"{newString}{newString}{newString}{newString}{newString}";
        }
    }
}
