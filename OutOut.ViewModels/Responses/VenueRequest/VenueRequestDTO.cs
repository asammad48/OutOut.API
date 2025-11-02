using OutOut.ViewModels.Responses.Venues;

namespace OutOut.ViewModels.Responses.VenueRequest
{
    public class VenueRequestDTO
    {
        public string Id { get; set; }
        public LastModificationRequestDTO lastModificationRequest { get; set; }
        public FullVenueResponse Venue { get; set; }
    }
}
