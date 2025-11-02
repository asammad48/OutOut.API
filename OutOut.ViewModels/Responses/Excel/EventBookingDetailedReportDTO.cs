namespace OutOut.ViewModels.Responses.Excel
{
    public class EventBookingDetailedReportDTO
    {
        public int OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public double Price { get; set; }
        public string CreationDate { get; set; }
        public string ReservationDate { get; set; }
        public string Email { get; set; }
        public long Attendees { get; set; }
        public long Absentees { get; set; }
    }
}
