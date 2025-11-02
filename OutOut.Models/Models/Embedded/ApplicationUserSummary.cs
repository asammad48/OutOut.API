using OutOut.Constants.Enums;
using System;

namespace OutOut.Models.Models.Embedded
{
    public class ApplicationUserSummary
    {
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Gender Gender { get; set; }
        public string ProfileImage { get; set; }
    }
}
