using System;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Venues
{
    public class AvailableTimeResponse
    {
        public List<DayOfWeek> Days { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
