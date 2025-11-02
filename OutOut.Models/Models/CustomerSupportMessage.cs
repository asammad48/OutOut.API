using OutOut.Constants.Enums;
using OutOut.Models.EntityInterfaces;
using System;

namespace OutOut.Models.Models
{
    public class CustomerSupportMessage : INonSqlEntity
    {
        public CustomerSupportMessage()
        {
            Status = CustomerSupportStatus.Pending;
            CreationDate = DateTime.UtcNow;
        }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public IssueTypes IssueType { get; set; }
        public string Description { get; set; }
        public CustomerSupportStatus Status { get; set; }
    }
}
