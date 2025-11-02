using System;
using System.Collections.Generic;

namespace OutOut.Models.Models
{
    public class AvailableTime
    {
        public List<DayOfWeek> Days { get; set; }
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }

        public static AvailableTime AlwaysAvailable() => new AvailableTime
        {
            Days = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
            From = new TimeSpan(00, 0, 0),
            To = new TimeSpan(23, 59, 59)
        };
    }
}
