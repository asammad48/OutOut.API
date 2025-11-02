using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OutOut.Models.EntityInterfaces
{
    [BsonIgnoreExtraElements]
    public class INonSqlEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}
