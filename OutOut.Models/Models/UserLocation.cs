using MongoDB.Driver.GeoJsonObjectModel;
using OutOut.Models.EntityInterfaces;

namespace OutOut.Models.Models
{
    public class UserLocation : ILocation
    {
        public UserLocation() { }
        public UserLocation(double longitude, double latitude, string description)
        {
            GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(longitude, latitude));
            Description = description;
        }
        public string Description { get; set; }
    }
}
