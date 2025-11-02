using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOut.ViewModels.Validators
{
    public class ValidCodeAttribute : ValidationAttribute
    {
        private static readonly Regex CodeRegex = new Regex(@"^\d{4}$");
        private readonly bool IsAlsoRequired;

        public ValidCodeAttribute(bool isAlsoRequired = true) { IsAlsoRequired = isAlsoRequired; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string code)
            {
                if (!string.IsNullOrEmpty(code))
                {
                    var isMatch = CodeRegex.IsMatch(code);
                    if (isMatch)
                        return ValidationResult.Success;

                    return new ValidationResult("Code must be 4 digits");
                }
                else
                {
                    if (IsAlsoRequired)
                        return new ValidationResult("Code is required.");
                    else
                        return ValidationResult.Success;
                }
            }

            else if (value is null)
            {
                if (IsAlsoRequired)
                    return new ValidationResult("Code is required.");
                else
                    return ValidationResult.Success;
            }

            return new ValidationResult("Invalid use of code attribute.");
        }
    }
}
