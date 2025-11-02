using OutOut.Constants.Enums;

namespace OutOut.ViewModels.Responses.Users
{
    public class ApplicationUserResponse
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public string ProfileImage { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public UserLocationResponse Location { get; set; }
        public bool NotificationsAllowed { get; set; }
        public bool RemindersAllowed { get; set; }
        public bool IsPasswordSet { get; set; }
    }
}
