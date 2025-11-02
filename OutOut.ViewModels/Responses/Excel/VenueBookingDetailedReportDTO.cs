namespace OutOut.ViewModels.Responses.Excel
{
    public class VenueBookingDetailedReportDTO
    {
        public int OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public long TotalReservations { get; set; }
        public string ReservationDate { get; set; }
        public string ReservationTime { get; set; }
        public string Phone { get; set; }
        public string CreationDate { get; set; }
        public string Status { get; set; }
    }
}
