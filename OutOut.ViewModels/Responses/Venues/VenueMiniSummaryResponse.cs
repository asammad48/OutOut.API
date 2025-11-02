using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Venues
{
    public class VenueMiniSummaryResponse
    {
        public string Id { get; set; }
        public string Logo { get; set; }
        public string DetailsLogo { get; set; }
        public string TableLogo { get; set; }
        public string Name { get; set; }
        public List<AvailableTimeResponse> OpenTimes { get; set; }
        public LocationResponse Location { get; set; }
    }
}
