using System.Text.RegularExpressions;

namespace OutOut.Core.Utils
{
    public static class StringUtils
    {
        private static readonly string[] KnownAcronyms = new string[] { };

        public static string CamelCaseToSpaces(string input)
        {
            if(KnownAcronyms.Length > 0)
            {
                var matchResult = Regex.Match(input, string.Join("|", KnownAcronyms));

                if (matchResult.Success == true)
                {
                    var firstPart = CamelCaseToSpaces(input.Substring(0, matchResult.Index));
                    var matchedAcronym = matchResult.Value;
                    var secondPart = CamelCaseToSpaces(input.Substring(matchedAcronym.Length + matchResult.Index));
                    return $"{firstPart} {matchedAcronym} {secondPart}";
                }
            }
            return Regex.Replace(input, "(\\B[A-Z])", " $1");
        }

        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, @"[^a-zA-Z0-9_'\s]+", "_", RegexOptions.Compiled);
        }
    }
}
