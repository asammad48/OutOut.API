using OutOut.ViewModels.Responses.Countries;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Cities
{
    public class CityResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Areas { get; set; }
        public bool IsActive { get; set; }
        public CountryResponse Country { get; set; }
    }
}
