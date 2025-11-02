using MongoDB.Driver.GeoJsonObjectModel;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models.Embedded;

namespace OutOut.Models.Models
{
    public class Location : ILocation
    {
        public Location() { }
        public Location(double longitude, double latitude, CitySummary city, string area, string description)
        {
            GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(longitude, latitude));
            City = city;
            Area = area;
            Description = description;
        }
        public CitySummary City { get; set; }
        public string Area { get; set; }
        public string Description { get; set; }
    }
}
