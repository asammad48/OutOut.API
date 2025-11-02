using OutOut.Models.EntityInterfaces;
using System.Collections.Generic;

namespace OutOut.Models.Models.Embedded
{
    public class VenueSummary : INonSqlEntity
    {
        public string Logo { get; set; }
        public string DetailsLogo { get; set; }
        public string TableLogo { get; set; }
        public string Name { get; set; }
        public List<AvailableTime> OpenTimes { get; set; }
        public Location Location { get; set; }
    }
}
