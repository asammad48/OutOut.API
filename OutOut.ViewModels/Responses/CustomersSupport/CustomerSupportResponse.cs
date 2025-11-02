using System;

namespace OutOut.ViewModels.Responses.CustomersSupport
{
    public class CustomerSupportResponse
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public IssueTypeResponse IssueType { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public CustomerSupportStatusResponse Status { get; set; }
    }

    public class IssueTypeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CustomerSupportStatusResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
