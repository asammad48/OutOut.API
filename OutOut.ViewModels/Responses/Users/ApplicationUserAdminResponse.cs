using OutOut.Constants.Enums;
using System.Collections.Generic;

namespace OutOut.ViewModels.Responses.Users
{
    public class ApplicationUserAdminResponse
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string ProfileImage { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public Gender Gender { get; set; }
        public List<string> AccessibleVenues { get; set; }
        public List<string> AccessibleEvents { get; set; }
    }
}
