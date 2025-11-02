using System.ComponentModel.DataAnnotations;

namespace OutOut.Constants.Enums
{
    public enum Availability
    {
        [Display(Name = "Active")]
        Active = 0,
        [Display(Name = "Inactive")]
        Inactive = 1,

        [Display(Name = "Area Deleted")]
        AreaDeleted = 2,

        [Display(Name = "City Deleted")]
        CityDeleted = 3,
        [Display(Name = "City Inactive")]
        CityInactive = 4,

        [Display(Name = "Venue Deleted")]
        VenueDeleted = 5,
        [Display(Name = "Venue Inactive")]
        VenueInactive = 6,
    }
}
