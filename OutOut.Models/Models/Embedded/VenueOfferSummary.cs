namespace OutOut.Models.Models.Embedded
{
    public class VenueOfferSummary
    {
        public VenueOfferSummary(string venueId, string venueName, Offer offer)
        {
            VenueId = venueId;
            VenueName = venueName;
            Offer = offer;
        }
        public string VenueId { get; set; }
        public string VenueName { get; set; }
        public Offer Offer { get; set; }
    }
}
