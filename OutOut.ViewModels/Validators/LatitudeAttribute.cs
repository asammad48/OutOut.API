using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOut.ViewModels.Validators
{
    public class LatitudeAttribute : ValidationAttribute
    {
        private static readonly Regex LatitudeRegex = new Regex(@"^(\+|-)?(?:90(?:(?:\.0{1,6})?)|(?:[0-9]|[1-8][0-9])(?:(?:\.[0-9]{1,6})?))$");
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is double latitude)
            {
                var isMatch = LatitudeRegex.IsMatch(latitude.ToString());
                if (isMatch)
                    return ValidationResult.Success;

                return new ValidationResult("Latitude is not valid.");
            }

            return new ValidationResult("Invalid use of latitude attribute.");
        }
    }
}
