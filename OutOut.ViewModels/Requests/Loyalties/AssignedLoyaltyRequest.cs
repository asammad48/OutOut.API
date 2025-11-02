using OutOut.Constants.Enums;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Validators;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Loyalties
{
    public class AssignedLoyaltyRequest
    {
        [Required]
        [MongoId]
        public string TypeId { get; set; }

        [Required]
        public LoyaltyStars Stars { get; set; }

        [MongoId]
        [Required]
        public string VenueId { get; set; }

        [Required]
        public List<AvailableTimeRequest> ValidOn { get; set; }

        [Required]
        public MaxUsage MaxUsage { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
