using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.TermsAndConditions
{
    public class TermsAndConditionsRequest
    {
        [MaxLength(1000)]
        [Required]
        public string TermCondition { get; set; }

        public bool IsActive { get; set; }
    }
}
