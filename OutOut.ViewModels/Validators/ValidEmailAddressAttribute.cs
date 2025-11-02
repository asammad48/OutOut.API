using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOut.ViewModels.Validators
{
    public class ValidEmailAddressAttribute : ValidationAttribute
    {
        private static readonly Regex EmailAddressRegex = new Regex(@"^(?:[a-z0-9_-]+(?:\.[a-z0-9_-]+)*)@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])+)$");

        public ValidEmailAddressAttribute() { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string || value == null)
            {
                var emailAddress = (string)value;

                if (string.IsNullOrEmpty(emailAddress))
                    return ValidationResult.Success;

                if (!string.IsNullOrEmpty(emailAddress))
                {
                    var isMatch = EmailAddressRegex.IsMatch(emailAddress.ToLower()) && StringInLengthLimit(emailAddress.ToLower());
                    if (isMatch)
                        return ValidationResult.Success;

                    return new ValidationResult("Email address not valid.");
                }
            }
            
            return new ValidationResult("Invalid use of email address attribute.");
        }

        private bool StringInLengthLimit(string emailAddress)
        {
            var localPart = emailAddress.Split('@')[0];
            return localPart.Length >= 1 && localPart.Length <= 64;
        }
    }
}
