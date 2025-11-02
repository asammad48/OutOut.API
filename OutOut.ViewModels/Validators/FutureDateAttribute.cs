using System;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Validators
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(value is DateTime))
                return new ValidationResult("invalid use of Date attribute");

            var currentValue = (DateTime)value;

            if (currentValue.Date < TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time")).Date)
                return new ValidationResult("Date should be in the future");

            return ValidationResult.Success;
        }
    }
}
