using OutOut.Models.EntityInterfaces;

namespace OutOut.Core.Utils
{
    public static class GeoCoordinateUtils
    {
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0;

            // Convert degrees to radians
            double lat1Rad = DegreesToRadians(lat1);
            double lat2Rad = DegreesToRadians(lat2);
            double deltaLat = DegreesToRadians(lat2 - lat1);
            double deltaLon = DegreesToRadians(lon2 - lon1);

            // Haversine formula
            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distanceKm = EarthRadiusKm * c;

            return Math.Round(distanceKm, 2);
        }

        public static double CalculateDistance(ILocation location1, ILocation location2)
        {
            return CalculateDistance(
                location1.GeoPoint.Coordinates.Latitude,
                location1.GeoPoint.Coordinates.Longitude,
                location2.GeoPoint.Coordinates.Latitude,
                location2.GeoPoint.Coordinates.Longitude);
        }

        public static bool IsInRange<T>(T data, double latitude, double longitude, double radius)
            where T : ILocation
        {
            var distance = CalculateDistance(
                latitude,
                longitude,
                data.GeoPoint.Coordinates.Latitude,
                data.GeoPoint.Coordinates.Longitude);

            return distance <= radius;
        }

        private static double DegreesToRadians(double degrees)
            => degrees * (Math.PI / 180.0);
    }
}
