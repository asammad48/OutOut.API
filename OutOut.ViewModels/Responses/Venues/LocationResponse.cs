using OutOut.ViewModels.Responses.Cities;

namespace OutOut.ViewModels.Responses.Venues
{
    public class LocationResponse
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public CitySummaryResponse City { get; set; }
        public string Area { get; set; }
        public string Description { get; set; }
        public double Distance { get; set; }
    }
}
