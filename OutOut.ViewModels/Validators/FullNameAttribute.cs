using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOut.ViewModels.Validators
{
    public class FullNameAttribute : ValidationAttribute
    {
        private static readonly Regex FullNameRegex = new Regex(@"^[a-zA-Z ]+$");
        public FullNameAttribute() { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is string)
            {
                var fullName = value as string;
                var match = FullNameRegex.Match(fullName);
                if (match.Success && fullName.Length >= 7 && fullName.Length <= 50 && !fullName.StartsWith(" "))
                    return ValidationResult.Success;
                else
                    return new ValidationResult("Full Name must be alphabet, can't start with space(s) & length between (10-50).");
            }

            return new ValidationResult("Invalid use of full name attribute.");
        }
    }
}
