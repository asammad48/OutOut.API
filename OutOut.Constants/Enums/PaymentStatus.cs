using System.ComponentModel.DataAnnotations;

namespace OutOut.Constants.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Unknown")]
        Unknown,
        [Display(Name = "Pending")]
        Pending,
        [Display(Name = "Paid")]
        Paid,
        [Display(Name = "Cancelled")]
        Cancelled,
        [Display(Name = "Declined")]
        Declined,
        [Display(Name = "Failed")]
        Failed,
        [Display(Name = "Aborted")]
        Aborted,
        [Display(Name = "Expired")]
        Expired,
        [Display(Name = "On Hold")]
        OnHold,
        [Display(Name = "Cancelled")]
        Rejected,
    }
}
