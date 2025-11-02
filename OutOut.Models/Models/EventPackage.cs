using OutOut.Models.EntityInterfaces;

namespace OutOut.Models.Models
{
    public class EventPackage : INonSqlEntity
    {
        public string Title { get; set; }
        public double Price { get; set; }
        public string Note { get; set; }
        public long TicketsNumber { get; set; }
        public long RemainingTickets { get; set; }
    }
}
