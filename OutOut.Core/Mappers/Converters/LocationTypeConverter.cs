using AutoMapper;
using OutOut.Core.Utils;
using OutOut.Models.Models;
using OutOut.Persistence.Providers;
using OutOut.ViewModels.Responses.Cities;
using OutOut.ViewModels.Responses.Venues;

namespace OutOut.Core.Mappers.Converters
{
    public class LocationTypeConverter : ITypeConverter<Location, LocationResponse>
    {
        public readonly IUserDetailsProvider _userDetailsProvider;

        public LocationTypeConverter(IUserDetailsProvider userDetailsProvider)
        {
            _userDetailsProvider = userDetailsProvider;
        }
      
        public LocationResponse Convert(Location source, LocationResponse destination, ResolutionContext context)
        {
            var user = _userDetailsProvider.User;
            return source == null || user.Location == null ? null : new LocationResponse
            {
                Latitude = source.GeoPoint.Coordinates.Latitude,
                Longitude = source.GeoPoint.Coordinates.Longitude,
                Distance = user.Location == null ? 0 : GeoCoordinateUtils.CalculateDistance(user.Location.GeoPoint.Coordinates.Latitude,
                                                                user.Location.GeoPoint.Coordinates.Longitude,
                                                                source.GeoPoint.Coordinates.Latitude,
                                                                source.GeoPoint.Coordinates.Longitude),
                City = new CitySummaryResponse {Id = source.City.Id, Name = source.City.Name, IsActive = source.City.IsActive },
                Area = source.Area,
                Description = source.Description,
            };
        }
    }
}
