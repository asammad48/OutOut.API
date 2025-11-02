using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Validators
{
    public class RequiredHostIfNoVenueSetAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentValue = (string)value;

            var property = validationContext.ObjectType.GetProperty("VenueId");

            if (property == null)
                return new ValidationResult("VenueId not found");

            var venueId = (string)property.GetValue(validationContext.ObjectInstance);

            if (string.IsNullOrEmpty(venueId) && string.IsNullOrEmpty(currentValue))
                return new ValidationResult("You need to choose a venue or enter host data");

            if ((!string.IsNullOrEmpty(venueId) && string.IsNullOrEmpty(currentValue)) || (string.IsNullOrEmpty(venueId) && !string.IsNullOrEmpty(currentValue)))
                return ValidationResult.Success;

            return new ValidationResult("Invalid use of host attribute.");
        }
    }

    public class RequiredHostImageIfNoVenueSetAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentValue = (IFormFile)value;

            var property = validationContext.ObjectType.GetProperty("VenueId");
            var setHostImageProperty = validationContext.ObjectType.GetProperty("SetHostImage");

            if (property == null)
                return new ValidationResult("VenueId not found");

            var venueId = (string)property.GetValue(validationContext.ObjectInstance);
            var setHostImage = (bool)setHostImageProperty.GetValue(validationContext.ObjectInstance);

            if (!setHostImage || (!string.IsNullOrEmpty(venueId) && currentValue == null) || (string.IsNullOrEmpty(venueId) && currentValue != null))
                return ValidationResult.Success;

            if (string.IsNullOrEmpty(venueId) && currentValue == null && setHostImage)
                return new ValidationResult("You need to choose a venue or enter host data");

            return new ValidationResult("Invalid use of host attribute.");
        }
    }
}
