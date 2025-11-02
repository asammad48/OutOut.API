namespace OutOut.ViewModels.Responses.Excel
{
    public class EventBookingResponseDTO
    {
        public string Date { get; set; }
        public int Quantity { get; set; }
        public string Time { get; set; }
        public double TotalAmount { get; set; }
        public string Location { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
    }

    public class EventBookingSummaryResponseDTO
    {
        public int OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string Date { get; set; }
        public string Package { get; set; }
        public string Status { get; set; }
    }

}
