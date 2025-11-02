using System;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Validators
{
    public class TimeLessThanAttribute : ValidationAttribute
    {
        private string EndTimePropert;
        public TimeLessThanAttribute(string EndTimePropert)
        {
            this.EndTimePropert = EndTimePropert;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentValue = (TimeSpan)value;

            var property = validationContext.ObjectType.GetProperty(EndTimePropert);

            if (property == null)
                return new ValidationResult("End Time not found");

            var endTime = (TimeSpan)property.GetValue(validationContext.ObjectInstance);

            if (currentValue > endTime)
                return new ValidationResult("End time should be greater than start time");

            else if (currentValue <= endTime)
                return ValidationResult.Success;

            return new ValidationResult("Invalid use of Time attribute.");
        }
    }
}
