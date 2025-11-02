using Microsoft.AspNetCore.Http;
using OutOut.Constants.Enums;
using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Customers
{
    public class CustomerUpdateAccountRequest
    {
        [MinLength(2)]
        [MaxLength(100)]
        public string FullName { get; set; }

        [ValidPhoneNumber]
        public string PhoneNumber { get; set; }

        public Gender Gender { get; set; }

        [ImageFile]
        public IFormFile ProfileImage { get; set; }
        public bool RemoveProfileImage { get; set; }
    }
}
