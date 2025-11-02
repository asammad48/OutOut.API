namespace OutOut.ViewModels.Responses.Events
{
    public class PackageOverviewReportResponse
    {
        public string Id { get; set; }
        public string PackageName { get; set; }
        public double NetPrice { get; set; }
        public long TotalTicketsBooked { get; set; }
        public long TotalTicketsCancelled { get; set; }
        public long TotalTicketsRemaining { get; set; }
        public double TotalSales { get; set; }
    }
}
