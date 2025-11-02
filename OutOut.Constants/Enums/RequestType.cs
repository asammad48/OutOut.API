using System.ComponentModel.DataAnnotations;

namespace OutOut.Constants.Enums
{
    public enum RequestType
    {
        [Display(Name = "Add Venue")]
        AddVenue,
        [Display(Name = "Add Event")]
        AddEvent,
        [Display(Name = "Assign Offer")]
        AssignOffer,
        [Display(Name = "Assign Loyalty")]
        AssignLoyalty,

        [Display(Name = "Update Venue")]
        UpdateVenue,
        [Display(Name = "Update Event")]
        UpdateEvent,
        [Display(Name = "Update Assigned Offer")]
        UpdateOffer,
        [Display(Name = "Update Assigned Loyalty")]
        UpdateLoyalty,

        [Display(Name = "Delete Venue")]
        DeleteVenue,
        [Display(Name = "Delete Event")]
        DeleteEvent,
        [Display(Name = "Unassign Offer")]
        UnassignOffer,
        [Display(Name = "Unassign All Offers")]
        UnassignAllOffers,
        [Display(Name = "Unassign All Loyalty")]
        UnassignAllLoyalty,
        [Display(Name = "Unassign Loyalty")]
        UnassignLoyalty,
    }
}
