using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOut.ViewModels.Validators
{
    public class UAEPhoneNumberAttribute : ValidationAttribute
    {
        private static readonly Regex LandlineNumberRegex = new Regex(@"(^0[234679])\d{7}$");
        private static readonly Regex MobileNumberRegex = new Regex(@"^\+971\d{9}$");

        public UAEPhoneNumberAttribute() { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string || value == null)
            {
                var phoneNumber = (string)value;

                if(string.IsNullOrEmpty(phoneNumber))
                    return ValidationResult.Success;

                var isMatch = LandlineNumberRegex.IsMatch(phoneNumber.ToLower()) || MobileNumberRegex.IsMatch(phoneNumber.ToLower());
                if (isMatch)
                    return ValidationResult.Success;

                return new ValidationResult("Phone number is not valid.");
            }

            return new ValidationResult("Invalid use of phone number attribute.");
        }
    }
}
