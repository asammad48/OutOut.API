namespace OutOut.ViewModels.Requests.Bookings
{
    public class BookingFilterationRequest
    {
        public string SearchQuery { get; set; }
        public SortBooking SortBy { get; set; }
    }

    public enum SortBooking
    {
        Newest, Alphabetical, Event, Venue 
    }
}
