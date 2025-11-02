using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOut.ViewModels.Validators
{
    public class NonArabicAttribute : ValidationAttribute
    {
        private const string ArabicRegex = @"\p{IsArabic}+";
        public NonArabicAttribute()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is string)
            {
                var text = value as string;

                if (Regex.Match(text, ArabicRegex).Success)
                    return new ValidationResult($"the field {validationContext.DisplayName} not accept arabic keywords");

                return ValidationResult.Success;
            }
            return new ValidationResult("Invalid use of  non Arabic attribute.");
        }


    }
}
