using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace OutOut.Models.EntityInterfaces
{
    [BsonIgnoreExtraElements]
    public abstract class ILocation
    {
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> GeoPoint { get; set; }
    }
}
