using OutOut.Models.EntityInterfaces;

namespace OutOut.Models.Models.Embedded
{
    public class EventSummary : INonSqlEntity
    {
        public string Name { get; set; }
        public string Image { get; set; }
    }
}
