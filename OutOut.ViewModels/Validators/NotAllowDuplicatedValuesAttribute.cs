using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OutOut.ViewModels.Validators
{
    class NotAllowDuplicatedValuesAttribute : ValidationAttribute
    {
        public NotAllowDuplicatedValuesAttribute() { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is IEnumerable<string>)
            {
                var values = value as IEnumerable<string>;
                if (values.Count() > 0 && values.Count() != values.Distinct().Count())
                {
                    return new ValidationResult($"List of {validationContext.DisplayName} not Allow duplicated values");
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid use of  NonNullableStringList attribute.");
        }
    }
}
