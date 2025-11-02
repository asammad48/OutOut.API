using Microsoft.AspNetCore.Http;
using OutOut.ViewModels.Validators;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Venues
{
    public class CreateVenueRequest
    {
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; }

        [MinLength(2)]
        [MaxLength(1000)]
        public string Description { get; set; }

        [ImageFile]
        [Required]
        public IFormFile Logo { get; set; }

        [ImageFile]
        [Required]
        public IFormFile Background { get; set; }

        [ImageFile]
        [Required]
        public IFormFile DetailsLogo { get; set; }

        [ImageFile]
        [Required]
        public IFormFile TableLogo { get; set; }

        [Required]
        public List<AvailableTimeRequest> AvailableTimes { get; set; }

        public LocationRequest Location { get; set; }

        [Required]
        public List<string> CategoriesIds { get; set; }

        [ValidCode]
        public string LoyaltyCode { get; set; }

        [ValidCode]
        public string OffersCode { get; set; }

        [ValidPhoneNumber(AllowTollFree: true)]
        public string PhoneNumber { get; set; }

        [DocumentFile]
        public IFormFile Menu { get; set; }

        public List<string> SelectedTermsAndConditions { get; set; }

        [ImageFiles]
        public List<IFormFile> Gallery { get; set; }

        public List<string> EventsIds { get; set; }

        [ValidUrl]
        public string FacebookLink { get; set; }
        [ValidUrl]
        public string InstagramLink { get; set; }

        [ValidUrl]
        public string YoutubeLink { get; set; }

        [ValidUrl]
        public string WebpageLink { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
