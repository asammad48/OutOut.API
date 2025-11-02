using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Venues
{
    public class AvailableTimeRequest
    {
        [Required]
        public List<DayOfWeek> Days { get; set; }

        [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
        [Required]
        public TimeSpan From { get; set; }

        [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
        [Required]
        public TimeSpan To { get; set; }
    }
}
