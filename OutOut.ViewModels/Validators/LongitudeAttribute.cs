using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOut.ViewModels.Validators
{
    public class LongitudeAttribute : ValidationAttribute
    {
        private static readonly Regex LongitudeRegex = new Regex(@"^(\+|-)?(?:180(?:(?:\.0{1,6})?)|(?:[0-9]|[1-9][0-9]|1[0-7][0-9])(?:(?:\.[0-9]{1,6})?))$");
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is double latitude)
            {
                var isMatch = LongitudeRegex.IsMatch(latitude.ToString());
                if (isMatch)
                    return ValidationResult.Success;

                return new ValidationResult("Longitude is not valid.");
            }

            return new ValidationResult("Invalid use of longitude attribute.");
        }
    }
}
