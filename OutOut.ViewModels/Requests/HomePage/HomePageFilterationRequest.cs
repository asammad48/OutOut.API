using System;
using System.Collections.Generic;

namespace OutOut.ViewModels.Requests.HomePage
{
    public class HomePageFilterationRequest
    {
        public string SearchQuery { set; get; }

        public List<string> VenueCategories { get; set; }
        public List<string> EventCategories { get; set; }
        public string OfferTypeId { get; set; }
        public string CityId { get; set; }
        public List<string> Areas { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
