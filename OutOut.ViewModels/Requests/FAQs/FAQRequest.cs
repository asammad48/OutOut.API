using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.FAQs
{
    public class FAQRequest
    {
        [Required]
        [MaxLength(500)]
        public string Question { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Answer { get; set; }
    }
}
