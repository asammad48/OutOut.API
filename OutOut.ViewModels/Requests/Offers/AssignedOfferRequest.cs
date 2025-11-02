using Microsoft.AspNetCore.Http;
using OutOut.Constants.Enums;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Offers
{
    public class AssignedOfferRequest
    {
        public bool SetImage { get; set; }

        [Required]
        [MongoId]
        public string TypeId { get; set; }

        [MongoId]
        [Required]
        public string VenueId { get; set; }

        [Required]
        public List<AvailableTimeRequest> ValidOn { get; set; }
        
        [Required]
        public OfferUsagePerYear MaxUsagePerYear { get; set; }

        [RequiredIfSetImage("SetImage")]
        [ImageFile]
        public IFormFile Image { get; set; }

        [Required]
        [FutureDate]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
