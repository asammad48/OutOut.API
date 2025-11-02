using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OutOut.Models;

namespace OutOut.Persistence.Data
{
    public class ApplicationNonSqlDbContext
    {
        private readonly AppSettings _appSettings;
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly Dictionary<string, string> _collectionsNames;

        public ApplicationNonSqlDbContext(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _collectionsNames = appSettings.Value.Connections.NonSqlCollectionsNames;
            if (_appSettings.Connections.NonSqlConnectionString != null)
            {
                _client = new MongoClient(_appSettings.Connections.NonSqlConnectionString);
                _database = _client.GetDatabase(_appSettings.Connections.NonSqlDatabaseName);
            }
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            string modelName = typeof(T).Name.ToString() + "CollectionName";
            string collectionName = _collectionsNames[modelName];
            return _database.GetCollection<T>(collectionName);
        }
    }
}