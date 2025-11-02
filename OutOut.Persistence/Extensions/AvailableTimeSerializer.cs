using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using OutOut.Models.Models;

namespace OutOut.Persistence.Extensions
{
    public class AvailableTimeSerializer : SerializerBase<AvailableTime>
    {
        public override AvailableTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            var document = serializer.Deserialize(context, args);
            var bsonDocument = document.ToBsonDocument();
            var result = BsonExtensionMethods.ToJson(bsonDocument);
            var availableTimeResult = JsonConvert.DeserializeObject<AvailableTime>(result);
            return availableTimeResult;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, AvailableTime value)
        {
            var effectiveAvailableTime = new AvailableTime
            {
                Days = value.Days,
                From = value.From,
                To = value.To
            };
            var jsonDocument = JsonConvert.SerializeObject(effectiveAvailableTime);

            if (effectiveAvailableTime.From > effectiveAvailableTime.To)
                throw new OutOutException(ErrorCodes.FromCannotBeGreaterThanTo);

            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);
            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            serializer.Serialize(context, bsonDocument.AsBsonValue);
        }
    }
}
