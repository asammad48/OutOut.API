using OutOut.ViewModels.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Events
{
    public class EventPackageRequest
    {
        [MongoId]
        public string Id { get; set; }

        [MaxLength(200)]
        [Required]
        public string Title { get; set; }

        [Range(typeof(double), "0.0", "9999999999.99")]
        [Required]
        public double Price { get; set; }

        [MaxLength(200)]
        public string Note { get; set; }

        [Range(0, 9999999999)]
        [Required]
        public long TicketsNumber { get; set; }
    }
}
