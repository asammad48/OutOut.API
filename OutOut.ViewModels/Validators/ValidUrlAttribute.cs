using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOut.ViewModels.Validators
{
    public class ValidUrlAttribute : ValidationAttribute
    {
        private readonly Regex regex = new Regex(@"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$");
        public ValidUrlAttribute() { }
        public override bool IsValid(object value)
        {
            var text = value as string;
            return string.IsNullOrEmpty(text) || regex.IsMatch(text.ToLower());
        }
    }
}
