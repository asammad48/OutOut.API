using OutOut.ViewModels.Responses.Venues;

namespace OutOut.ViewModels.Responses.VenueRequest
{
    public class VenueRequestSummaryDTO
    {
        public string Id { get; set; }
        public LastModificationRequestDTO lastModificationRequest { get; set; }
        public VenueSummaryWithBookingResponse Venue { get; set; }
    }
}
