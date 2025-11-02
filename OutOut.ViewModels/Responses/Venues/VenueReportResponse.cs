namespace OutOut.ViewModels.Responses.Venues
{
    public class VenueReportResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long ApprovedBookingsCount { get; set; } //Tables Booked
        public long CancelledBookingsCount { get; set; }
        public long TotalBookingsCount { get; set; } //Total Reservation
        public long LoyaltyUsageCount { get; set; }
    }
}
