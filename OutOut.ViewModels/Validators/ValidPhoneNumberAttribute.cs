using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOut.ViewModels.Validators
{
    public class ValidPhoneNumberAttribute : ValidationAttribute
    {
        private static readonly Regex PhoneNumberRegexWithTollFree = new Regex(@"^\+971\d{3,13}$");
        private static readonly Regex PhoneNumberRegex = new Regex(@"^\+971\d{9}$");
        private readonly bool allowTollFree;

        public ValidPhoneNumberAttribute(bool AllowTollFree = false) { allowTollFree = AllowTollFree; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string || value == null)
            {
                var phoneNumber = (string)value;

                if (string.IsNullOrEmpty(phoneNumber))
                    return ValidationResult.Success;
                bool isMatch;

                if (allowTollFree)
                    isMatch = PhoneNumberRegexWithTollFree.IsMatch(phoneNumber.ToLower());
                else
                    isMatch = PhoneNumberRegex.IsMatch(phoneNumber.ToLower());

                if (isMatch)
                    return ValidationResult.Success;

                return new ValidationResult("Phone number is not valid.");
            }

            return new ValidationResult("Invalid use of phone number attribute.");
        }
    }
}
