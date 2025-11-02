using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Areas
{
    public class UpdateAreaRequest
    {
        public string OldArea { get; set; }

        [MaxLength(50)]
        [Required]
        public string NewArea { get; set; }
    }
}
