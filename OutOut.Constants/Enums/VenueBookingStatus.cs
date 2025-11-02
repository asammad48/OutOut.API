using System.ComponentModel.DataAnnotations;

namespace OutOut.Constants.Enums
{
    public enum VenueBookingStatus
    {
        [Display(Name = "Pending")]
        Pending,
        [Display(Name = "Approved")]
        Approved,
        [Display(Name = "Rejected")]
        Rejected,
        [Display(Name = "Cancelled")]
        Cancelled
    }
}
