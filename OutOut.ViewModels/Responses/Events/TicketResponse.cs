using OutOut.Models.Models.Embedded;

namespace OutOut.ViewModels.Responses.Events
{
    public class TicketResponse
    {
        public long? Index { get; set; }
        public string Id { get; set; }
        public EventPackageSummary Package { get; set; }
        public string Secret { get; set; }
        public string UserId { get; set; }
        public DateTime? RedemptionDate { get; set; }
        public string Status { get; set; }
    }
}
