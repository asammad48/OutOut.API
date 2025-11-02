using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.DeveloperTools
{
    public class SystemEnumsResponse
    {
        public IEnumerable<EnumResponse> DatabaseEnums { get; set; }
        public IEnumerable<EnumResponse> FilterationEnums { get; set; }
        public IEnumerable<EnumResponse> ConstantEnums { get; set; }
    }
    public class EnumResponse
    {
        public string EnumName { get; set; }
        public Dictionary<string, int> EnumValues { get; set; } = new Dictionary<string, int>();
    }
}
