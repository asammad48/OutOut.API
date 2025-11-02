using OutOut.ViewModels.Validators;
using System;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Events
{
    public class EventOccurrenceRequest
    {
        [MongoId]
        public string Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        [EndDate("StartDate")]
        public DateTime EndDate { get; set; }

        [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
        [Required]
        public TimeSpan StartTime { get; set; }

        [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
