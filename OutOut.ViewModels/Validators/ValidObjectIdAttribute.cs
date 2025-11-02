using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Validators
{
    public class ValidObjectIdAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentProprety = validationContext.ObjectType.GetProperty(validationContext.MemberName);

            var stringValue = (string)value;

            if (string.IsNullOrEmpty(stringValue))
                return new ValidationResult("Id must be provided.");

            if (!ObjectId.TryParse(stringValue, out _))
                return new ValidationResult("Invalid database Id.");

            return ValidationResult.Success;
        }
    }
}
