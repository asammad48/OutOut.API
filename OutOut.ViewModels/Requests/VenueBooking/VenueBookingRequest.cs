using System;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.VenueBooking
{
    public class VenueBookingRequest
    {
        [Required]
        public string VenueId { get; set; }
        [Required]
        [Range(1,20)]
        public int PeopleNumber { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}
