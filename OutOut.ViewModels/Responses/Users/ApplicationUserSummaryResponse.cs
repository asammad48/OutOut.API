using OutOut.Constants.Enums;

namespace OutOut.ViewModels.Responses.Users
{
    public class ApplicationUserSummaryResponse
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Gender Gender { get; set; }
        public string ProfileImage { get; set; }
    }
}
