using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using OutOut.Models.Models;

namespace OutOut.Persistence.Extensions
{
    public class EventOccurrenceTimeSerializer : SerializerBase<EventOccurrence>
    {
        public override EventOccurrence Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            var document = serializer.Deserialize(context, args);
            var bsonDocument = document.ToBsonDocument();
            var eventOccurrenceResult = new EventOccurrence
            {
                Id = bsonDocument.GetValue("Id").AsObjectId.ToString(),
                StartDate = bsonDocument.GetValue("StartDate").ToUniversalTime().Date,
                EndDate = bsonDocument.GetValue("EndDate").ToUniversalTime().Date,
                StartTime = TimeSpan.Parse(bsonDocument.GetValue("StartTime").AsString),
                EndTime = TimeSpan.Parse(bsonDocument.GetValue("EndTime").AsString),
                Packages = bsonDocument.GetValue("Packages").AsBsonArray.Values.Select(a => BsonSerializer.Deserialize<EventPackage>(a.AsBsonDocument)).ToList()
            };
            return eventOccurrenceResult;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, EventOccurrence value)
        {
            var bsonDocument = new BsonDocument
            {
                { "Id", new ObjectId(value.Id) },
                { "StartDate", new BsonDateTime(value.StartDate) },
                { "EndDate", new BsonDateTime(value.EndDate) },
                { "StartTime", new BsonString(value.StartTime.ToString())},
                { "EndTime", new BsonString(value.EndTime.ToString())},
                { "Packages", new BsonArray(value.Packages.Select(p => p.ToBsonDocument()))},
            };
            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            serializer.Serialize(context, bsonDocument);
        }
    }
}
