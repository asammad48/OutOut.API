using System;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Events
{
    public class EventOccurrenceResponse
    {
        public string Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public List<EventPackageResponse> Packages { get; set; }
    }
}
