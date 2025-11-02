using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;

namespace OutOut.Models.Models.Embedded
{
    public class LastModificationRequest
    {
        public LastModificationRequest(string createdBy, RequestType type, string modifiedFieldId)
        {
            Date = DateTime.UtcNow;
            CreatedBy = createdBy;
            Type = type;
            ModifiedFieldId = modifiedFieldId;
        }
        public LastModificationRequest()
        {
            Date = DateTime.UtcNow;
        }

        public DateTime Date { get; set; }
        public string CreatedBy { get; set; }
        public RequestType Type { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ModifiedFieldId { get; set; }
    }
}
