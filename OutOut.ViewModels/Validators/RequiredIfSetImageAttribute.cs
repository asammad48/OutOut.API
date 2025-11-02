using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Validators
{
    public class RequiredIfSetImageAttribute : ValidationAttribute
    {
        private readonly string _flagField;

        public RequiredIfSetImageAttribute(string flagField)
        {
            _flagField = flagField;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentValue = (IFormFile)value;

            var property = validationContext.ObjectType.GetProperty(_flagField);

            if (property == null)
                return new ValidationResult($"{_flagField} not found");

            var setImage = (bool)property.GetValue(validationContext.ObjectInstance);

            if (setImage && currentValue == null)
                return new ValidationResult("Image is required");

            if ((!setImage && currentValue == null) || (setImage && currentValue != null))
                return ValidationResult.Success;

            return new ValidationResult("Invalid use of image attribute.");
        }
    }
}
