using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.EventBooking
{
    public class EventBookingRequest
    {
        [Required]
        [MongoId]
        public string EventOccurrenceId { get; set; }
        [Required]
        [Range(1, 10)]
        public int Quantity { get; set; }
        [Required]
        [MongoId]
        public string PackageId { get; set; }
        [Required]
        public double TotalAmount { get; set; }
    }
}
