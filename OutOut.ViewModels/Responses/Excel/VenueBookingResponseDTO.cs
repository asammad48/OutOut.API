namespace OutOut.ViewModels.Responses.Excel
{
    public class VenueBookingResponseDTO
    {
        public string Date { get; set; }
        public int NumberOfPeople { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
    }

    public class VenueBookingSummaryResponseDTO
    {
        public int OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public int TablesBooked { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
    }

}
