namespace OutOut.ViewModels.Responses.Events
{
    public class TelrBookingResponse
    {
        public TelrBookingResponse(string bookingUrl, string bookingId)
        {
            BookingUrl = bookingUrl;
            BookingId = bookingId; 
        }
        public string BookingUrl { get; set; }
        public string BookingId { get; set; }
    }
}
