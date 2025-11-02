using System.ComponentModel.DataAnnotations;

namespace OutOut.Constants.Enums
{
    public enum Gender
    {
        [Display(Name = "Female")]
        Female = 0,
        [Display(Name = "Male")]
        Male = 1,
        [Display(Name = "Prefer not to specify")]
        Unspecified = 2,
    }
}
