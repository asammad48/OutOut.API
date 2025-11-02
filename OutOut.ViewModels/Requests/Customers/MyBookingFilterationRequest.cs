namespace OutOut.ViewModels.Requests.Customers
{
    public class MyBookingFilterationRequest
    {
        public string SearchQuery { get; set; }
        public MyBookingFilteration MyBooking { get; set; }
    }
    public enum MyBookingFilteration
    {
        Recent, History
    }
}
