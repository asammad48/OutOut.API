using OutOut.Constants.Enums;
using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.CustomersSupport
{
    public class CustomerSupportRequest
    {
        [MinLength(2)]
        [MaxLength(100)]
        public string FullName { get; set; }

        [ValidPhoneNumber]
        public string PhoneNumber { get; set; }

        public IssueTypes IssueType { get; set; }

        [MinLength(2)]
        [MaxLength(2000)]
        public string Description { get; set; }
    }
}
