namespace OutOut.ViewModels.Responses.Events
{
    public class EventReportResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsEnded { get; set; }
        public long TicketsBooked { get; set; }
        public long TicketsLeft { get; set; }
        public double Revenue { get; set; }
        public long Attendees { get; set; }
        public long Absentees { get; set; }
    }
}
