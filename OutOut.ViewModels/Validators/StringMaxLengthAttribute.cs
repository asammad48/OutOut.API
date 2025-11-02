using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Validators
{
    public class StringMaxLengthAttribute : StringLengthAttribute
    {
        public StringMaxLengthAttribute(int maximumLength) : base(maximumLength) { }
        public override bool IsValid(object value)
        {
            if (value is null)
                return true;

            if (value is not List<string>)
                return false;

            foreach (var str in value as List<string>)
            {
                if (str.Length > MaximumLength || str.Length < MinimumLength)
                    return false;
            }

            return true;
        }
    }
}
