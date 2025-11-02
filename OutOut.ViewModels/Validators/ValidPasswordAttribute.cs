using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOut.ViewModels.Validators
{
    public class ValidPasswordAttribute : ValidationAttribute
    {
        private static readonly Regex PasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W]).{8,25}$");

        public ValidPasswordAttribute() { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string || value == null)
            {
                var password = (string)value;

                if (string.IsNullOrEmpty(value as string))
                    return ValidationResult.Success;

                var isMatch = PasswordRegex.IsMatch(password);
                if (isMatch)
                    return ValidationResult.Success;

                return new ValidationResult("Your password must be more than 8 symbols, less than 25 symbols and needs to have a minimum of one small letter, one capital letter, one symbol, and one number.");
            }

            return new ValidationResult("Invalid use of password attribute.");
        }
    }
}
