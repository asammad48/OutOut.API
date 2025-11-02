using System;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Validators
{
    public class EndDateAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        public EndDateAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(value is DateTime))
                return new ValidationResult("invalid use of Date attribute");

            var currentValue = (DateTime)value;
            
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
                return new ValidationResult($"Property with this name {_comparisonProperty} not found");

            var propertyValue = property.GetValue(validationContext.ObjectInstance);
            DateTime comparisonValue = (DateTime)propertyValue;

            if (currentValue < comparisonValue)
                return new ValidationResult("End date should be greater or equal to start date");

            return ValidationResult.Success;
        }
    }
}
