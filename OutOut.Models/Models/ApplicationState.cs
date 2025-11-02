using MongoDB.Bson;

namespace OutOut.Models.Models
{
    public class ApplicationState
    {
        public ObjectId Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
