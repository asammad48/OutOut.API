using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OutOut.Constants.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDisplayName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                                    .GetMember(enumValue.ToString())
                                    .First()
                                    .GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}
