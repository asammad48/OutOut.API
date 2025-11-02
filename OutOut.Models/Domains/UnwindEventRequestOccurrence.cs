using MongoDB.Bson.Serialization.Attributes;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Domains
{
    [BsonIgnoreExtraElements]
    public class UnwindEventRequestOccurrence : INonSqlEntity
    {
        public LastModificationRequest LastModificationRequest { get; set; }
        public SingleEventOccurrence Event { get; set; }
        public SingleEventOccurrence OldEvent { get; set; }
    }
}
