using Microsoft.AspNetCore.Http;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Validators;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Events
{
    public class UpsertEventRequest
    {
        public bool SetImage { get; set; }
        public bool SetHeaderImage { get; set; }
        public bool SetTableLogo { get; set; }
        public bool SetDetailsLogo { get; set; }

        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; }

        [MinLength(2)]
        [MaxLength(1000)]
        public string Description { get; set; }

        [ImageFile]
        [RequiredIfSetImage("SetImage")]
        public IFormFile Image { get; set; }

        [ImageFile]
        [RequiredIfSetImage("SetHeaderImage")]
        public IFormFile HeaderImage { get; set; }

        [ImageFile]
        [RequiredIfSetImage("SetTableLogo")]
        public IFormFile TableLogo { get; set; }

        [ImageFile]
        [RequiredIfSetImage("SetDetailsLogo")]
        public IFormFile DetailsLogo { get; set; }

        public LocationRequest Location { get; set; }

        [ValidPhoneNumber(AllowTollFree: true)]
        public string PhoneNumber { get; set; }

        [ValidEmailAddress]
        public string Email { get; set; }

        [Required]
        public List<string> CategoriesIds { get; set; }

        [ValidUrl]
        public string FacebookLink { get; set; }

        [ValidUrl]
        public string InstagramLink { get; set; }

        [ValidUrl]
        public string YoutubeLink { get; set; }

        [ValidUrl]
        public string WebpageLink { get; set; }

        public bool IsFeatured { get; set; } = false;

        public List<EventPackageRequest> Packages { get; set; }

        public List<EventOccurrenceRequest> Occurrences { get; set; }

        [MongoId]
        public string VenueId { get; set; }

        public bool SetHostImage { get; set; }

        [MaxLength(100)]
        public string HostedBy { get; set; }

        [ImageFile]
        public IFormFile HostImage { get; set; }

        public bool IsActive { get; set; } = true;

        [ValidCode(isAlsoRequired: false)]
        public string Code { get; set; }
    }
}
