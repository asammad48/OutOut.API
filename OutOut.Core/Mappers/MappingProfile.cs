using OutOut.Infrastructure.Services;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using System;
using OutOut.Models;
using OutOut.ViewModels.Responses.TermsAndConditions;
using OutOut.ViewModels.Requests.TermsAndConditions;
using OutOut.ViewModels.Requests.Users;
using OutOut.ViewModels.Responses.Categories;
using OutOut.Models.Wrappers;
using System.Linq;
using OutOut.Constants.Extensions;
using OutOut.ViewModels.Requests.Auth;
using OutOut.ViewModels.Responses.Venues;
using OutOut.ViewModels.Responses.Users;
using OutOut.ViewModels.Responses.FAQs;
using OutOut.ViewModels.Responses.CustomersSupport;
using OutOut.ViewModels.Responses.Customers;
using OutOut.ViewModels.Requests.FAQs;
using OutOut.ViewModels.Requests.CustomersSupport;
using OutOut.ViewModels.Requests.Categories;
using OutOut.ViewModels.Requests.Venues;
using OutOut.ViewModels.Responses.Loyalties;
using OutOut.ViewModels.Requests.Loyalties;
using OutOut.Models.Models.Embedded;
using OutOut.ViewModels.Requests.VenueBooking;
using OutOut.ViewModels.Responses.VenueBooking;
using OutOut.ViewModels.Responses.Offers;
using OutOut.Models.Domain;
using OutOut.Core.Mappers.Converters;
using OutOut.ViewModels.Responses.Events;
using OutOut.Models.Domains;
using OutOut.ViewModels.Responses.Notifications;
using OutOut.Core.Services;
using OutOut.ViewModels.Responses.Cities;
using OutOut.ViewModels.Responses.EventBookings;
using OutOut.ViewModels.Responses.Countries;
using OutOut.ViewModels.Requests.Cities;
using OutOut.ViewModels.Responses.LoyaltyTypes;
using OutOut.ViewModels.Responses.OfferTypes;
using OutOut.ViewModels.Requests.LoyaltyTypes;
using OutOut.ViewModels.Requests.OfferTypes;
using OutOut.ViewModels.Requests.AdminProfile;
using OutOut.ViewModels.Requests.Events;
using MongoDB.Bson;
using OutOut.ViewModels.Responses.VenueRequest;
using OutOut.ViewModels.Responses.EventRequest;
using OutOut.ViewModels.Responses.Excel;
using System.Collections.Generic;
using OutOut.Models.Utils;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Presentation;
using OutOut.Constants.Enums;
using OutOut.Persistence.Providers;

namespace OutOut.Core.Mappers
{
    public class MappingProfile : AutoMapper.Profile
    {
        private readonly AppSettings _appSettings;

        string GetFullPath(string directory, string logoPath)
        {
            if (string.IsNullOrWhiteSpace(logoPath))
            {
                return "";
            }
            return _appSettings.BackendOrigin + "/" + directory + "/" + logoPath;
        }

        public MappingProfile(AppSettings appSettings)
        {
            _appSettings = appSettings;

            // User
            CreateMap<UpdateAdminAccountRequest, ApplicationUser>();
            CreateMap<Page<ApplicationUserSummary>, Page<ApplicationUserSummaryResponse>>();
            CreateMap<ApplicationUserSummary, ApplicationUserSummaryResponse>()
              .ForMember(c => c.ProfileImage, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.ProfileImages, src.ProfileImage)));
            CreateMap<ApplicationUser, ApplicationUserSummaryResponse>()
               .ForMember(c => c.ProfileImage, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.ProfileImages, src.ProfileImage)));
            CreateMap<Page<ApplicationUser>, Page<ApplicationUserSummaryResponse>>();
            CreateMap<AdminProfileRequest, ApplicationUser>()
                .ForMember(dest => dest.UserName, options => options.MapFrom(src => Guid.NewGuid().ToString()));
            CreateMap<ApplicationUser, ApplicationUserAdminResponse>()
               .ForMember(c => c.ProfileImage, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.ProfileImages, src.ProfileImage)));
            CreateMap<Page<ApplicationUser>, Page<ApplicationUserAdminResponse>>();

            CreateMap<ApplicationUser, ApplicationUserResponse>()
               .ForMember(c => c.ProfileImage, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.ProfileImages, src.ProfileImage)))
               .ForMember(c => c.IsPasswordSet, opt => opt.MapFrom(src => src.PasswordHash != null));
            CreateMap<CustomerRegistrationRequest, ApplicationUser>()
                .ForMember(dest => dest.UserName, options => options.MapFrom(src => Guid.NewGuid().ToString()));
            CreateMap<UserLocation, UserLocationResponse>()
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.GeoPoint.Coordinates.Longitude))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.GeoPoint.Coordinates.Latitude));
            CreateMap<UserLocationRequest, UserLocation>()
                 .ConstructUsing((src, res) =>
                 {
                     return new UserLocation(src.Longitude, src.Latitude, src.Description);
                 });

            CreateMap<ApplicationUser, CustomerDTO>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToDisplayName()));

            CreateMap<ApplicationUserSummary, CustomerDTO>()
               .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.FullName))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber))
               .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToDisplayName()));

            CreateMap<ApplicationUser, ApplicationUserSummary>();
            CreateMap<OTPResult, UserOTP>();
            CreateMap<TimeSpan, OTPVerificationTimeLeftResponse>()
                .ForMember(dest => dest.Minutes, options => options.MapFrom((src, _) => src.Minutes >= 0 ? src.Minutes : 0))
                .ForMember(dest => dest.Seconds, options => options.MapFrom((src, _) => src.Seconds >= 0 ? src.Seconds : 0));

            //FAQ
            CreateMap<FAQ, FAQResponse>();
            CreateMap<Page<FAQ>, Page<FAQResponse>>();
            CreateMap<FAQRequest, FAQ>();

            //TermsAndConditions
            CreateMap<TermsAndConditions, TermsAndConditionsResponse>();
            CreateMap<Page<TermsAndConditions>, Page<TermsAndConditionsResponse>>();
            CreateMap<TermsAndConditionsRequest, TermsAndConditions>();

            //Category
            CreateMap<Page<Category>, Page<CategoryResponse>>();
            CreateMap<Category, CategoryResponse>()
               .ForMember(c => c.Icon, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.CategoryIcons, src.Icon)));
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<UpdateCategoryRequest, Category>()
                .ForMember(c => c.Icon, opt => opt.Ignore());

            // Customer Support
            CreateMap<CustomerSupportRequest, CustomerSupportMessage>();
            CreateMap<Page<CustomerSupportMessage>, Page<CustomerSupportResponse>>();
            CreateMap<CustomerSupportMessage, CustomerSupportResponse>()
                .ForPath(dest => dest.Status.Id, opt => opt.MapFrom(src => (int)src.Status))
                .ForPath(dest => dest.Status.Name, opt => opt.MapFrom(src => src.Status.ToDisplayName()))
                .ForPath(dest => dest.IssueType.Id, opt => opt.MapFrom(src => (int)src.IssueType))
                .ForPath(dest => dest.IssueType.Name, opt => opt.MapFrom(src => src.IssueType.ToDisplayName()));

            //Venues
            CreateMap<AvailableTimeRequest, AvailableTime>();
            CreateMap<Location, LocationResponse>().ConvertUsing<LocationTypeConverter>();
            CreateMap<AvailableTime, AvailableTimeResponse>()
                .ForMember(ot => ot.From, opt => opt.MapFrom(src => To12HrFormat(src.From)))
                .ForMember(ot => ot.To, opt => opt.MapFrom(src => To12HrFormat(src.To)));
            CreateMap<Venue, VenueResponse>()
                .ForMember(c => c.IsFavorite, opt => opt.ConvertUsing<FavoriteVenuesValueConverter, string>(src => src.Id))
                .ForMember(c => c.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
                .ForMember(c => c.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.DetailsLogo)))
                .ForMember(c => c.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.TableLogo)))
                .ForMember(c => c.Menu, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesMenus, src.Menu)))
                .ForMember(c => c.Gallery, opt => opt.MapFrom(src => src.Gallery.Select(a => GetFullPath(appSettings.Directories.VenuesGallery, a))))
                .ForMember(c => c.Background, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesGallery, src.Background)))
                .ForMember(c => c.Categories, opt => opt.MapFrom(src => src.Categories.Where(a => a.IsActive == true)))
                .ForMember(c => c.Loyalty, opt => opt.Ignore())
                .ForMember(c => c.Offers, opt => opt.Ignore());
            CreateMap<Page<Venue>, Page<VenueResponse>>();

            CreateMap<VenueOneOffer, VenueSummary>();
            CreateMap<Venue, VenueSummary>();
            CreateMap<VenueSummary, VenueMiniSummaryResponse>()
                .ForMember(c => c.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
                .ForMember(c => c.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.DetailsLogo)))
                .ForMember(c => c.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.TableLogo)));
            CreateMap<Venue, VenueMiniSummaryResponse>()
                .ForMember(c => c.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)));
            CreateMap<LastModificationRequest, LastModificationRequestDTO>()
              .ForMember(c => c.Type, opt => opt.MapFrom(src => src.Type.ToDisplayName()));
            CreateMap<VenueRequest, VenueRequestDTO>();
            CreateMap<Page<VenueRequest>, Page<VenueRequestSummaryDTO>>();
            CreateMap<VenueRequest, VenueRequestSummaryDTO>()
                 .ForPath(c => c.Venue.Id, opt => opt.MapFrom(src => src.OldVenue != null ? src.OldVenue.Id : src.Venue.Id))
                 .ForPath(c => c.Venue.Name, opt => opt.MapFrom(src => src.OldVenue != null ? src.OldVenue.Name : src.Venue.Name))
                 .ForPath(c => c.Venue.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.OldVenue != null ? src.OldVenue.Logo : src.Venue.Logo)))
                 .ForPath(c => c.Venue.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.OldVenue != null ? src.OldVenue.DetailsLogo : src.Venue.DetailsLogo)))
                 .ForPath(c => c.Venue.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Venue.TableLogo)))
                 .ForPath(c => c.Venue.Description, opt => opt.MapFrom(src => src.OldVenue != null ? src.OldVenue.Description : src.Venue.Description))
                 .ForPath(c => c.Venue.OpenTimes, opt => opt.MapFrom(src => src.OldVenue != null ? src.OldVenue.OpenTimes : src.Venue.OpenTimes))
                 .ForPath(c => c.Venue.Location, opt => opt.MapFrom(src => src.OldVenue != null ? src.OldVenue.Location : src.Venue.Location))
                 .ForPath(c => c.Venue.PhoneNumber, opt => opt.MapFrom(src => src.OldVenue != null ? src.OldVenue.PhoneNumber : src.Venue.PhoneNumber))
                 .ForPath(c => c.Venue.Status, opt => opt.MapFrom(src => src.OldVenue != null ? src.OldVenue.Status.ToDisplayName() : src.Venue.Status.ToDisplayName()))
                 .ForPath(c => c.Venue.Categories, opt => opt.MapFrom(src => src.OldVenue != null ? src.OldVenue.Categories.Where(a => a.IsActive == true) : src.Venue.Categories.Where(a => a.IsActive == true)));
            CreateMap<Venue, FullVenueResponse>()
                .ForMember(c => c.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
                .ForMember(c => c.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.DetailsLogo)))
                .ForMember(c => c.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.TableLogo)))
                .ForMember(c => c.Menu, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesMenus, src.Menu)))
                .ForMember(c => c.Gallery, opt => opt.MapFrom(src => src.Gallery.Select(a => GetFullPath(appSettings.Directories.VenuesGallery, a))))
                .ForMember(c => c.Background, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesGallery, src.Background)))
                .ForMember(c => c.Loyalty, opt => opt.MapFrom(src => src.Loyalty))
                .ForMember(c => c.Status, opt => opt.MapFrom(src => src.Status.ToDisplayName()))
                .ForMember(c => c.Categories, opt => opt.MapFrom(src => src.Categories.Where(a => a.IsActive == true)))
                .ForMember(c => c.BookingsCount, opt => opt.ConvertUsing<VenueApprovedBookingsCountValueConverter, string>(src => src.Id));
            CreateMap<Venue, VenueSummaryWithBookingResponse>()
              .ForMember(c => c.Status, opt => opt.MapFrom(src => src.Status.ToDisplayName()))
              .ForMember(c => c.Count, opt => opt.ConvertUsing<VenueApprovedBookingsCountValueConverter, string>(src => src.Id))
              .ForMember(c => c.Categories, opt => opt.MapFrom(src => src.Categories.Where(a => a.IsActive == true)))
              .ForMember(c => c.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
              .ForMember(c => c.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.DetailsLogo)))
              .ForMember(c => c.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.TableLogo)));
            CreateMap<Page<Venue>, Page<VenueSummaryWithBookingResponse>>();

            CreateMap<Venue, VenueSummaryResponse>()
              .ForMember(c => c.IsFavorite, opt => opt.ConvertUsing<FavoriteVenuesValueConverter, string>(src => src.Id))
              .ForMember(c => c.Categories, opt => opt.MapFrom(src => src.Categories.Where(a => a.IsActive == true)))
              .ForMember(c => c.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
              .ForMember(c => c.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.DetailsLogo)))
              .ForMember(c => c.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.TableLogo)));
            CreateMap<Page<Venue>, Page<VenueSummaryResponse>>();

            CreateMap<VenueWithDistance, VenueSummaryResponse>()
             .ForMember(c => c.IsFavorite, opt => opt.ConvertUsing<FavoriteVenuesValueConverter, string>(src => src.Id))
             .ForMember(c => c.Categories, opt => opt.MapFrom(src => src.Categories.Where(a => a.IsActive == true)))
             .ForMember(c => c.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
             .ForMember(c => c.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.DetailsLogo)))
              .ForMember(c => c.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.TableLogo)));
            CreateMap<Page<VenueWithDistance>, Page<VenueSummaryResponse>>();

            CreateMap<CreateVenueRequest, Venue>()
                .ForMember(c => c.Location, opt => opt.Ignore())
                .ForMember(c => c.Logo, opt => opt.Ignore())
                .ForMember(c => c.Background, opt => opt.Ignore())
                .ForMember(c => c.Menu, opt => opt.Ignore())
                .ForMember(c => c.Gallery, opt => opt.Ignore());

            CreateMap<UpdateVenueRequest, Venue>()
                .ForMember(c => c.Location, opt => opt.Ignore())
                .ForMember(c => c.Logo, opt => opt.Ignore())
                .ForMember(c => c.DetailsLogo, opt => opt.Ignore())
                .ForMember(c => c.TableLogo, opt => opt.Ignore())
                .ForMember(c => c.Background, opt => opt.Ignore())
                .ForMember(c => c.Menu, opt => opt.Ignore())
                .ForMember(c => c.Gallery, opt => opt.Ignore());

            //VenueBooking
            CreateMap<VenueBookingRequest, VenueBooking>();
            CreateMap<VenueBooking, VenueBookingResponseDTO>()
                .ForMember(c => c.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(c => c.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(c => c.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(c => c.Gender, opt => opt.MapFrom(src => src.User.Gender.ToDisplayName()))
                .ForMember(c => c.NumberOfPeople, opt => opt.MapFrom(src => src.PeopleNumber))
                .ForMember(c => c.Date, opt => opt.MapFrom(src => src.Date.ToString("dd MMM yyyy")))
                .ForMember(c => c.Time, opt => opt.MapFrom(src => src.Date.ToString("h:mm tt")))
                .ForMember(c => c.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(c => c.Location, opt => opt.MapFrom(src => src.Venue.Location != null ? src.Venue.Location.Area + ", " + src.Venue.Location.City.Name : ""))
                .ForMember(c => c.Status, opt => opt.MapFrom(src => src.Status.ToDisplayName()));
            CreateMap<VenueBooking, VenueBookingSummaryResponseDTO>()
                .ForMember(c => c.OrderNumber, opt => opt.MapFrom(src => src.BookingNumber))
                .ForMember(c => c.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(c => c.TablesBooked, opt => opt.MapFrom(src => src.PeopleNumber))
                .ForMember(c => c.Date, opt => opt.MapFrom(src => src.Date.ToString("dd MMM yyyy")))
                .ForMember(c => c.Time, opt => opt.MapFrom(src => src.Date.ToString("h:mm tt")))
                .ForMember(c => c.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(c => c.Status, opt => opt.MapFrom(src => src.Status.ToDisplayName()));
            CreateMap<VenueBooking, VenueBookingResponse>();
            CreateMap<Page<VenueBooking>, Page<VenueBookingResponse>>();
            CreateMap<VenueBooking, VenueBookingSummaryResponse>()
                .ForMember(c => c.RemindersTypes, opt => opt.MapFrom(src => src.Reminders.ToList()));
            CreateMap<VenueBooking, VenueBookingSummary>()
                .ForMember(c => c.VenueId, src => src.MapFrom(src => src.Venue.Id))
                .ForMember(c => c.VenueName, src => src.MapFrom(src => src.Venue.Name))
                .ForMember(c => c.VenueLogo, src => src.MapFrom(src => src.Venue.Logo));

            //LoyaltyType
            CreateMap<LoyaltyType, LoyaltyTypeSummaryResponse>();
            CreateMap<LoyaltyType, LoyaltyTypeResponse>()
                .ForMember(c => c.UsageCount, opt => opt.ConvertUsing<LoyaltyTypeUsageCountValueConverter, string>(src => src.Id))
                .ForMember(c => c.AssignmentCount, opt => opt.ConvertUsing<LoyaltyTypeAssignmentCountValueConverter, string>(src => src.Id));
            CreateMap<Page<LoyaltyType>, Page<LoyaltyTypeResponse>>();
            CreateMap<CreateLoyaltyTypeRequest, LoyaltyType>();
            CreateMap<LoyaltyTypeRequest, LoyaltyType>();

            //Loyalty
            CreateMap<Venue, AssignedLoyaltyResponse>()
                .ForMember(c => c.Id, src => src.MapFrom(src => src.Loyalty.Id))
                .ForMember(c => c.Type, opt => opt.MapFrom(src => src.Loyalty.Type))
                .ForMember(c => c.Stars, src => src.MapFrom(src => src.Loyalty.Stars))
                .ForMember(c => c.IsActive, src => src.MapFrom(src => src.Loyalty.IsActive))
                .ForMember(c => c.MaxUsage, src => src.MapFrom(src => src.Loyalty.MaxUsage))
                .ForMember(c => c.ValidOn, src => src.MapFrom(src => src.Loyalty.ValidOn))
                .ForPath(c => c.Venue.Id, src => src.MapFrom(src => src.Id))
                .ForPath(c => c.Venue.Name, opt => opt.MapFrom(src => src.Name))
                .ForPath(c => c.Venue.Categories, src => src.MapFrom(src => src.Categories))
                .ForPath(c => c.Venue.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
                .ForPath(c => c.Venue.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.DetailsLogo)))
                .ForPath(c => c.Venue.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.TableLogo)))
                .ForPath(c => c.Venue.OpenTimes, src => src.MapFrom(src => src.OpenTimes))
                .ForPath(c => c.Venue.Location, src => src.MapFrom(src => src.Location))
                .ForPath(c => c.Venue.PhoneNumber, src => src.MapFrom(src => src.PhoneNumber));
            CreateMap<Page<Venue>, Page<AssignedLoyaltyResponse>>();

            CreateMap<AssignedLoyaltyRequest, Loyalty>();

            CreateMap<Venue, VenueLoyaltySummary>();
            CreateMap<VenueLoyaltySummary, VenueMiniSummaryResponse>()
                .ForPath(c => c.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
                .ForPath(c => c.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.TableLogo)))
                .ForPath(c => c.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.DetailsLogo)));

            CreateMap<Page<UserLoyalty>, Page<CustomerLoyaltyResponse>>();
            CreateMap<UserLoyalty, CustomerLoyaltyResponse>()
                .ForMember(c => c.Type, opt => opt.MapFrom(src => src.Loyalty.Type))
                .ForMember(c => c.RedemptionsCount, opt => opt.ConvertUsing<CustomerLoyaltyRedemptionCountValueConverter, UserLoyalty>(src => src));

            CreateMap<Loyalty, LoyaltyResponse>()
                .ForMember(c => c.IsApplicable, opt => opt.ConvertUsing<ApplicableLoyaltyValueConverter, Loyalty>(src => src));

            CreateMap<AvailableTime, AvailableTimeResponse>()
                .ForPath(c => c.Days, src => src.MapFrom(src => src.Days))
                .ForPath(c => c.From, src => src.MapFrom(src => To12HrFormat(src.From)))
                .ForPath(c => c.To, src => src.MapFrom(src => To12HrFormat(src.To)));
            CreateMap<Page<UserLoyalty>, Page<LoyaltyResponse>>();
            CreateMap<UserLoyalty, LoyaltyResponse>()
                .ForMember(c => c.Id, src => src.MapFrom(src => src.Loyalty.Id))
                .ForMember(c => c.Type, opt => opt.MapFrom(src => src.Loyalty.Type))
                .ForMember(c => c.Stars, src => src.MapFrom(src => src.Loyalty.Stars))
                .ForMember(c => c.StarsCount, src => src.MapFrom(src => LoyaltyService.GetStarsCount(src)))
                .ForMember(c => c.IsActive, src => src.MapFrom(src => src.Loyalty.IsActive))
                .ForMember(c => c.IsApplicable, opt => opt.ConvertUsing<ApplicableLoyaltyValueConverter, UserLoyalty>(src => src))
                .ForMember(c => c.MaxUsage, src => src.MapFrom(src => src.Loyalty.MaxUsage))
                .ForMember(c => c.ValidOn, src => src.MapFrom(src => src.Loyalty.ValidOn));

            CreateMap<UserLoyalty, VenueLoyaltySummaryResponse>()
                .ForMember(c => c.Id, src => src.MapFrom(src => src.Loyalty.Id))
                .ForMember(c => c.Type, opt => opt.MapFrom(src => src.Loyalty.Type))
                .ForMember(c => c.Stars, src => src.MapFrom(src => src.Loyalty.Stars))
                .ForMember(c => c.StarsCount, src => src.MapFrom(src => LoyaltyService.GetStarsCount(src)))
                .ForMember(c => c.IsActive, src => src.MapFrom(src => src.Loyalty.IsActive))
                .ForMember(c => c.IsApplicable, opt => opt.ConvertUsing<ApplicableLoyaltyValueConverter, UserLoyalty>(src => src))
                .ForMember(c => c.CanGet, src => src.MapFrom(src => src.CanGet))
                .ForMember(c => c.MaxUsage, src => src.MapFrom(src => src.Loyalty.MaxUsage))
                .ForMember(c => c.ValidOn, src => src.MapFrom(src => src.Loyalty.ValidOn));

            CreateMap<Loyalty, VenueLoyaltySummaryResponse>()
                .ForMember(c => c.IsApplicable, opt => opt.ConvertUsing<ApplicableLoyaltyValueConverter, Loyalty>(src => src));

            CreateMap<UserLoyaltyRequest, UserLoyalty>();

            //OfferTypes
            CreateMap<OfferType, OfferTypeSummaryResponse>();
            CreateMap<OfferType, OfferTypeResponse>()
                .ForMember(c => c.UsageCount, opt => opt.ConvertUsing<OfferTypeUsageCountValueConverter, string>(src => src.Id))
                .ForMember(c => c.AssignmentCount, opt => opt.ConvertUsing<OfferTypeAssignmentCountValueConverter, string>(src => src.Id));
            CreateMap<Page<OfferType>, Page<OfferTypeResponse>>();
            CreateMap<OfferTypeRequest, OfferType>();

            //Offers
            CreateMap<Page<UserOffer>, Page<CustomerOfferResponse>>();
            CreateMap<UserOffer, CustomerOfferResponse>()
                .ForMember(c => c.Type, opt => opt.MapFrom(src => src.Offer.Type))
                .ForMember(c => c.RedemptionsCount, opt => opt.ConvertUsing<CustomerOfferRedemptionCountValueConverter, UserOffer>(src => src));

            CreateMap<VenueOneOffer, OfferResponse>()
                .ForMember(c => c.Id, opt => opt.MapFrom(src => src.Offer.Id))
                .ForMember(c => c.Type, opt => opt.MapFrom(src => src.Offer.Type))
                .ForMember(c => c.MaxUsagePerYear, opt => opt.MapFrom(src => src.Offer.MaxUsagePerYear))
                .ForMember(c => c.Image, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.OffersImages, src.Offer.Image)))
                .ForMember(c => c.IsActive, src => src.MapFrom(src => src.Offer.IsActive))
                .ForMember(c => c.ExpiryDate, src => src.MapFrom(src => src.Offer.ExpiryDate))
                .ForMember(c => c.ValidOn, src => src.MapFrom(src => src.Offer.ValidOn))
                .ForMember(c => c.IsApplicable, opt => opt.ConvertUsing<ApplicableOfferValueConverter, Offer>(src => src.Offer));
            CreateMap<Page<VenueOneOffer>, Page<OfferResponse>>();

            CreateMap<VenueOneOffer, OfferWithUsageResponse>()
               .ForMember(c => c.Id, opt => opt.MapFrom(src => src.Offer.Id))
               .ForMember(c => c.Type, opt => opt.MapFrom(src => src.Offer.Type))
               .ForMember(c => c.Image, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.OffersImages, src.Offer.Image)))
               .ForMember(c => c.IsActive, src => src.MapFrom(src => src.Offer.IsActive))
               .ForMember(c => c.ExpiryDate, src => src.MapFrom(src => src.Offer.ExpiryDate))
               .ForMember(c => c.MaxUsagePerYear, src => src.MapFrom(src => src.Offer.MaxUsagePerYear))
               .ForMember(c => c.ValidOn, src => src.MapFrom(src => src.Offer.ValidOn))
               .ForMember(c => c.Count, opt => opt.ConvertUsing<OfferUsageCountValueConverter, string>(src => src.Offer.Id))
               .ForPath(dest => dest.Venue.Id, opt => opt.MapFrom(src => src.Id))
               .ForPath(dest => dest.Venue.Location, opt => opt.MapFrom(src => src.Location))
               .ForPath(dest => dest.Venue.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
               .ForPath(dest => dest.Venue.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.DetailsLogo)))
               .ForPath(dest => dest.Venue.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.TableLogo)))
               .ForPath(dest => dest.Venue.Name, opt => opt.MapFrom(src => src.Name))
               .ForPath(dest => dest.Venue.OpenTimes, opt => opt.MapFrom(src => src.OpenTimes))
               .ForPath(dest => dest.Venue.Categories, opt => opt.MapFrom(src => src.Categories));
            CreateMap<Page<VenueOneOffer>, Page<OfferWithUsageResponse>>();

            CreateMap<VenueOneOffer, OfferWithVenueResponse>()
                .ForMember(c => c.Id, opt => opt.MapFrom(src => src.Offer.Id))
                .ForMember(c => c.Type, opt => opt.MapFrom(src => src.Offer.Type))
                .ForMember(c => c.Image, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.OffersImages, src.Offer.Image)))
                .ForMember(c => c.IsActive, src => src.MapFrom(src => src.Offer.IsActive))
                .ForMember(dest => dest.NextAvailableDate, opt => opt.MapFrom(src => GetNextDayOccurance(UAEDateTime.Now, src.Offer.ValidOn)))
                .ForPath(dest => dest.Venue.Id, opt => opt.MapFrom(src => src.Id))
                .ForPath(dest => dest.Venue.Location, opt => opt.MapFrom(src => src.Location))
                .ForPath(dest => dest.Venue.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
                .ForPath(dest => dest.Venue.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.DetailsLogo)))
                .ForPath(dest => dest.Venue.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.TableLogo)))
                .ForPath(dest => dest.Venue.Name, opt => opt.MapFrom(src => src.Name))
                .ForPath(dest => dest.Venue.OpenTimes, opt => opt.MapFrom(src => src.OpenTimes))
                .ForPath(dest => dest.Venue.Categories, opt => opt.MapFrom(src => src.Categories));
            CreateMap<Page<VenueOneOffer>, Page<OfferWithVenueResponse>>();

            CreateMap<VenueOneOfferWithDistance, OfferWithVenueResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Offer.Id))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.OffersImages, src.Offer.Image)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Offer.Type))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Offer.IsActive))
                .ForMember(dest => dest.NextAvailableDate, opt => opt.MapFrom(src => GetNextDayOccurance(UAEDateTime.Now, src.Offer.ValidOn)))
                .ForPath(dest => dest.Venue.Id, opt => opt.MapFrom(src => src.Id))
                .ForPath(dest => dest.Venue.Location, opt => opt.MapFrom(src => src.Location))
                .ForPath(dest => dest.Venue.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Logo)))
                .ForPath(dest => dest.Venue.Name, opt => opt.MapFrom(src => src.Name))
                .ForPath(dest => dest.Venue.OpenTimes, opt => opt.MapFrom(src => src.OpenTimes))
                .ForPath(dest => dest.Venue.Categories, opt => opt.MapFrom(src => src.Categories));
            CreateMap<Page<VenueOneOfferWithDistance>, Page<OfferWithVenueResponse>>();
            CreateMap<Page<Offer>, Page<OfferResponse>>();
            CreateMap<Offer, OfferResponse>()
                .ForMember(c => c.Image, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.OffersImages, src.Image)))
                .ForMember(c => c.IsApplicable, opt => opt.ConvertUsing<ApplicableOfferValueConverter, Offer>(src => src))
                .ForMember(c => c.NextAvailableDate, opt => opt.MapFrom(src => GetNextDayOccurance(UAEDateTime.Now, src.ValidOn)));
            CreateMap<UserOffer, OfferResponse>()
                .ForMember(c => c.Id, src => src.MapFrom(src => src.Offer.Id))
                .ForMember(c => c.Image, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.OffersImages, src.Offer.Image)))
                .ForMember(c => c.Type, src => src.MapFrom(src => src.Offer.Type))
                .ForMember(c => c.IsActive, src => src.MapFrom(src => src.Offer.IsActive))
                .ForMember(c => c.ExpiryDate, src => src.MapFrom(src => src.Offer.ExpiryDate))
                .ForMember(c => c.ValidOn, src => src.MapFrom(src => src.Offer.ValidOn))
                .ForMember(c => c.MaxUsagePerYear, src => src.MapFrom(src => src.Offer.MaxUsagePerYear))
                .ForMember(c => c.IsApplicable, opt => opt.ConvertUsing<ApplicableOfferValueConverter, UserOffer>(src => src))
                .ForMember(c => c.NextAvailableDate, opt => opt.MapFrom(src => GetNextDayOccurance(UAEDateTime.Now, src.Offer.ValidOn)));


            CreateMap<Page<UnwindVenueRequestOffer>, Page<OfferWithUsageResponse>>();
            CreateMap<UnwindVenueRequestOffer, OfferWithUsageResponse>()
                .ForMember(c => c.Id, opt => opt.MapFrom(src => src.Venue.Offer.Id))
                .ForMember(c => c.Type, opt => opt.MapFrom(src => src.Venue.Offer.Type))
                .ForMember(c => c.Image, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.OffersImages, src.Venue.Offer.Image)))
                .ForMember(c => c.IsActive, src => src.MapFrom(src => src.Venue.Offer.IsActive))
                .ForMember(c => c.ExpiryDate, src => src.MapFrom(src => src.Venue.Offer.ExpiryDate))
                .ForMember(c => c.MaxUsagePerYear, src => src.MapFrom(src => src.Venue.Offer.MaxUsagePerYear))
                .ForMember(c => c.ValidOn, src => src.MapFrom(src => src.Venue.Offer.ValidOn))
                .ForPath(dest => dest.Venue.Id, opt => opt.MapFrom(src => src.Venue.Id))
                .ForPath(dest => dest.Venue.Location, opt => opt.MapFrom(src => src.Venue.Location))
                .ForPath(dest => dest.Venue.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Venue.Logo)))
                .ForPath(dest => dest.Venue.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Venue.DetailsLogo)))
                .ForPath(dest => dest.Venue.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Venue.TableLogo)))
                .ForPath(dest => dest.Venue.Name, opt => opt.MapFrom(src => src.Venue.Name))
                .ForPath(dest => dest.Venue.OpenTimes, opt => opt.MapFrom(src => src.Venue.OpenTimes))
                .ForPath(dest => dest.Venue.Categories, opt => opt.MapFrom(src => src.Venue.Categories));

            CreateMap<UserOffer, OfferWithUsageResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Offer.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Offer.Type))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.OffersImages, src.Offer.Image)))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Offer.IsActive))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.Offer.ExpiryDate))
                .ForMember(dest => dest.MaxUsagePerYear, opt => opt.MapFrom(src => src.Offer.MaxUsagePerYear))
                .ForMember(dest => dest.ValidOn, opt => opt.MapFrom(src => src.Offer.ValidOn))
                .ForMember(c => c.Count, opt => opt.ConvertUsing<CustomerOfferRedemptionCountValueConverter, UserOffer>(src => src))
                .ForPath(dest => dest.Venue.Id, opt => opt.MapFrom(src => src.Venue.Id))
                .ForPath(dest => dest.Venue.Location, opt => opt.MapFrom(src => src.Venue.Location))
                .ForPath(dest => dest.Venue.Logo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.VenuesLogos, src.Venue.Logo)))
                .ForPath(dest => dest.Venue.Name, opt => opt.MapFrom(src => src.Venue.Name))
                .ForPath(dest => dest.Venue.OpenTimes, opt => opt.MapFrom(src => src.Venue.OpenTimes));

            CreateMap<Page<UserOffer>, Page<OfferWithUsageResponse>>();

            //Events
            CreateMap<EventRequest, EventRequestDTO>();
            CreateMap<EventRequest, EventRequestSummaryDTO>()
                 .ForPath(c => c.Event.Id, opt => opt.MapFrom(src => src.OldEvent != null ? src.OldEvent.Id : src.Event.Id))
                 .ForPath(c => c.Event.Name, opt => opt.MapFrom(src => src.OldEvent != null ? src.OldEvent.Name : src.Event.Name))
                 .ForPath(c => c.Event.Image, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.OldEvent != null ? src.OldEvent.Image : src.Event.Image)))
                 .ForPath(c => c.Event.HeaderImage, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.OldEvent != null ? src.OldEvent.HeaderImage : src.Event.HeaderImage)))
                 .ForPath(c => c.Event.TableLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.Event.TableLogo)))
                 .ForPath(c => c.Event.DetailsLogo, opt => opt.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.OldEvent != null ? src.OldEvent.DetailsLogo : src.Event.DetailsLogo)))
                 .ForPath(c => c.Event.Description, opt => opt.MapFrom(src => src.OldEvent != null ? src.OldEvent.Description : src.Event.Description))
                 .ForPath(c => c.Event.Location, opt => opt.MapFrom(src => src.OldEvent != null ? src.OldEvent.Location : src.Event.Location))
                 .ForPath(c => c.Event.PhoneNumber, opt => opt.MapFrom(src => src.OldEvent != null ? src.OldEvent.PhoneNumber : src.Event.PhoneNumber))
                 .ForPath(c => c.Event.Occurrence, opt => opt.MapFrom(src => src.OldEvent != null ? GetNearestOccurrenceInDate(src.OldEvent.Occurrences) : GetNearestOccurrenceInDate(src.Event.Occurrences)))
                 .ForPath(c => c.Event.IsFeatured, opt => opt.MapFrom(src => src.OldEvent != null ? src.OldEvent.IsFeatured : src.Event.IsFeatured))
                 .ForPath(c => c.Event.Status, opt => opt.MapFrom(src => src.OldEvent != null ? src.OldEvent.Status.ToDisplayName() : src.Event.Status.ToDisplayName()));

            CreateMap<Page<EventRequest>, Page<EventRequestSummaryDTO>>();

            CreateMap<UpsertEventRequest, Event>()
                .ForMember(c => c.Location, opt => opt.Ignore())
                .ForMember(c => c.Occurrences, opt => opt.Ignore())
                .ForMember(c => c.Venue, opt => opt.Ignore())
                .ForMember(c => c.HeaderImage, opt => opt.Ignore())
                .ForMember(c => c.TableLogo, opt => opt.Ignore())
                .ForMember(c => c.DetailsLogo, opt => opt.Ignore())
                .ForMember(c => c.Image, opt => opt.Ignore())
                ;
            CreateMap<Event, FullEventResponse>()
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToDisplayName()))
               .ForMember(dest => dest.RemainingTicketsCount, opt => opt.ConvertUsing<EventRemainingTicketsCountValueConverter, Event>(src => src))
               .ForMember(dest => dest.PendingTicketsCount, opt => opt.ConvertUsing<EventPendingTicketsCountValueConverter, Event>(src => src))
               .ForMember(dest => dest.BookedTicketsCount, opt => opt.ConvertUsing<EventBookedTicketsCountValueConverter, Event>(src => src))
               .ForMember(dest => dest.Revenue, opt => opt.ConvertUsing<EventRevenueValueConverter, string>(src => src.Id))
               .ForMember(dest => dest.Image, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.Image)))
               .ForMember(dest => dest.HeaderImage, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.HeaderImage)))
               .ForMember(dest => dest.TableLogo, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.TableLogo)))
               .ForMember(dest => dest.DetailsLogo, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.DetailsLogo)))
               .ForMember(dest => dest.Packages, src => src.MapFrom(src => src.Occurrences.SelectMany(a => a.Packages).GroupBy(a => a.Id).Select(a => a.FirstOrDefault()).ToList()))
               .ForMember(c => c.Categories, opt => opt.MapFrom(src => src.Categories.Where(a => a.IsActive == true)))
               .AfterMap((src, dest) => dest.Occurrences = dest.Occurrences.OrderBy(p => p.StartDate).ToList())
               .AfterMap((src, dest) => dest.Packages = dest.Packages.OrderBy(p => p.Price).ToList());

            CreateMap<EventPackageRequest, EventPackage>();
            CreateMap<EventPackage, EventPackageResponse>();
            CreateMap<EventPackageSummary, EventPackageSummaryResponse>();
            CreateMap<EventPackage, EventPackageSummary>();
            CreateMap<EventOccurrence, EventOccurrenceResponse>()
                .ForPath(c => c.StartTime, opt => opt.MapFrom(src => To12HrFormat(src.StartTime)))
                .ForPath(c => c.EndTime, opt => opt.MapFrom(src => To12HrFormat(src.EndTime)))
                .AfterMap((src, dest) => dest.Packages = dest.Packages.OrderBy(p => p.Price).ToList());

            CreateMap<EventOccurrenceRequest, EventOccurrence>()
                .ForMember(c => c.Id, opt => opt.MapFrom(src => src.Id ?? ObjectId.GenerateNewId().ToString()))
                .ForMember(c => c.StartDate, src => src.MapFrom(src => src.StartDate.Date))
                .ForMember(c => c.EndDate, src => src.MapFrom(src => src.EndDate.Date));

            CreateMap<SingleEventOccurrence, SingleEventOccurrenceResponse>()
                .ForMember(c => c.IsFavorite, opt => opt.MapFrom((src, dest, destMember, context) => {
                    var userDetailsProvider = (IUserDetailsProvider)context.Items["UserDetailsProvider"];
                    return userDetailsProvider.User?.FavoriteEvents.Contains(src.Occurrence.Id) ?? false;
                }))
                .ForMember(dest => dest.Image, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.Image)))
                .ForMember(c => c.Categories, opt => opt.MapFrom(src => src.Categories.Where(a => a.IsActive == true)))
                .AfterMap((src, dest) => dest.Occurrence.Packages = dest.Occurrence.Packages.OrderBy(p => p.Price).ToList());

            CreateMap<Event, SingleEventOccurrenceResponse>()
                .ForMember(c => c.Categories, opt => opt.MapFrom(src => src.Categories.Where(a => a.IsActive == true)))
                .ForMember(dest => dest.Image, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.Image)));

            CreateMap<Event, EventMiniSummaryResponse>()
                .ForMember(dest => dest.Image, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.Image)));

            CreateMap<Event, EventSummary>()
                .ForMember(dest => dest.Image, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.Image)));
            CreateMap<Event, EventSummaryResponse>()
                .ForMember(dest => dest.Image, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.Image)))
                .ForMember(dest => dest.EventVenueName, src => src.MapFrom((s, d, _) => string.IsNullOrEmpty(s.Venue?.Name) ? s.Name : $"{s.Name} ({s.Venue.Name})"))
                .ForMember(dest => dest.VenueName, src => src.MapFrom((s, d, _) => s.Venue?.Name));
            CreateMap<SingleEventOccurrence, EventSummaryResponse>()
                .ForMember(c => c.IsFavorite, opt => opt.ConvertUsing<FavoriteEventsValueConverter, string>(src => src.Occurrence.Id))
                .ForMember(dest => dest.Image, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.Image)))
                .ForMember(dest => dest.DetailsLogo, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.DetailsLogo)))
                .ForMember(dest => dest.TableLogo, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.TableLogo)));
            CreateMap<Page<SingleEventOccurrence>, Page<EventSummaryResponse>>();

            CreateMap<SingleEventOccurrence, EventSummaryWithBookingResponse>()
               .ForMember(c => c.Status, opt => opt.MapFrom(src => src.Status.ToDisplayName()))
               .ForMember(c => c.VenueName, opt => opt.MapFrom((s, d, _) => s.Venue?.Name))
               .ForMember(c => c.Count, opt => opt.ConvertUsing<EventBookedTicketsCountValueConverter, SingleEventOccurrence>(src => src))
               .ForMember(dest => dest.Image, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.Image)))
               .ForMember(dest => dest.HeaderImage, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.HeaderImage)))
               .ForMember(dest => dest.DetailsLogo, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.DetailsLogo)))
               .ForMember(dest => dest.TableLogo, src => src.MapFrom(src => GetFullPath(appSettings.Directories.EventsImages, src.TableLogo)));
            CreateMap<Page<SingleEventOccurrence>, Page<EventSummaryWithBookingResponse>>();

            CreateMap<Ticket, TicketResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<Page<SingleEventBookingTicket>, Page<TicketResponse>>();
            CreateMap<SingleEventBookingTicket, TicketResponse>()
               .ForMember(c => c.Id, opt => opt.MapFrom(src => src.Ticket.Id))
               .ForMember(c => c.Package, opt => opt.MapFrom(src => src.Ticket.Package))
               .ForMember(c => c.RedemptionDate, opt => opt.MapFrom(src => src.Ticket.RedemptionDate))
               .ForMember(c => c.Secret, opt => opt.MapFrom(src => src.Ticket.Secret))
               .ForMember(c => c.UserId, opt => opt.MapFrom(src => src.User.Id))
               .ForMember(c => c.Status, opt => opt.MapFrom(src => src.Ticket.Status.ToString()));
            CreateMap<Page<SingleEventBookingTicket>, Page<TicketDetails>>();
            CreateMap<SingleEventBookingTicket, TicketDetails>()
                .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.OrderNumber))
                .ForMember(dest => dest.EventName, opt => opt.MapFrom(src => src.Event.Name))
                .ForMember(dest => dest.ReservationDate, opt => opt.MapFrom(src => src.Event.Occurrence.StartDate))
                //.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                //.ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                //.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.RedemptionDate, opt => opt.MapFrom(src => src.Ticket.RedemptionDate))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.BookingLocation, opt => opt.MapFrom(src => src.Event.Location))
                //.ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => To12HrFullFormat(src.Event.Occurrence.StartTime)))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => To12HrFullFormat(src.Event.Occurrence.EndTime)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Ticket.Status.ToString()))
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.Ticket.RejectionReason));

            CreateMap<EventBooking, EventBookingResponseDTO>()
               .ForMember(c => c.Date, opt => opt.MapFrom(src => src.Event.Occurrence.StartDate.ToString("dd MMM yyyy")))
               .ForMember(c => c.Time, opt => opt.MapFrom(src => To12HrFullFormat(src.Event.Occurrence.StartTime)))
               .ForMember(c => c.Location, opt => opt.MapFrom(src => src.Event.Location != null ? src.Event.Location.Area + ", " + src.Event.Location.City : ""))
               .ForMember(c => c.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
               .ForMember(c => c.Email, opt => opt.MapFrom(src => src.User.Email))
               .ForMember(c => c.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
               .ForMember(c => c.Gender, opt => opt.MapFrom(src => src.User.Gender.ToDisplayName()));

            CreateMap<EventBooking, EventBookingSummaryResponseDTO>()
               .ForMember(c => c.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
               .ForMember(c => c.Date, opt => opt.MapFrom(src => src.Event.Occurrence.StartDate.ToString("dd MMM yyyy")))
               .ForMember(c => c.Package, opt => opt.MapFrom(src => src.Package.Title))
               .ForMember(c => c.Status, opt => opt.MapFrom(src => src.Status.ToDisplayName()));

            CreateMap<EventBooking, EventBookingSummaryResponse>();
            CreateMap<Page<EventBooking>, Page<EventBookingSummaryResponse>>();

            CreateMap<SingleEventBookingTicket, SingleEventBookingTicketSummaryResponse>();
            CreateMap<Page<SingleEventBookingTicket>, Page<SingleEventBookingTicketSummaryResponse>>();

            CreateMap<EventBooking, EventBookingMiniSummaryResponse>()
                .AfterMap((src, dest) => dest.Tickets = dest.Tickets.OrderBy(p => p.Id).ToList());

            CreateMap<EventBooking, EventBookingSummary>()
               .ForMember(c => c.EventId, src => src.MapFrom(src => src.Event.Id))
               .ForMember(c => c.EventOccurrenceId, src => src.MapFrom(src => src.Event.Occurrence.Id))
               .ForMember(c => c.EventName, src => src.MapFrom(src => src.Event.Name))
               .ForMember(c => c.EventImage, src => src.MapFrom(src => src.Event.Image));

            CreateMap<Page<EventBooking>, Page<CustomerEventBookingResponse>>();
            CreateMap<EventBooking, CustomerEventBookingResponse>()
                .ForMember(c => c.EventId, opt => opt.MapFrom(src => src.Event.Id))
                .ForMember(c => c.EventOccurrenceId, opt => opt.MapFrom(src => src.Event.Occurrence.Id))
                .ForMember(c => c.EventName, opt => opt.MapFrom(src => src.Event.Name))
                .ForMember(c => c.PackageName, opt => opt.MapFrom(src => src.Package.Title))
                .ForMember(c => c.CityName, opt => opt.MapFrom(src => src.Event.Location != null ? src.Event.Location.City.Name : null))
                .ForMember(c => c.EventOccurrenceStartDate, opt => opt.MapFrom(src => src.Event.Occurrence.StartDate));

            //Notification
            CreateMap<Notification, NotificationResponse>()
               .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Action == Constants.Enums.NotificationAction.NewOffer ? GetFullPath(appSettings.Directories.OffersImages, src.Image) : GetFullPath(appSettings.Directories.NotificationsIcons, src.Image)))
               .ForMember(dest => dest.UpdatedEntityId,
               opt =>
               {
                   opt.MapFrom(src => src.Venue != null ? src.Venue.Id : src.Event != null ? src.Event.Id : src.Offer != null ? src.Offer.Offer.Id : null);
               });

            CreateMap<Page<Notification>, Page<NotificationResponse>>();

            CreateMap<Notification, NotificationAdminResponse>();
            CreateMap<CustomNotificationPage<Notification>, CustomNotificationPage<NotificationAdminResponse>>();


            //City
            CreateMap<CreateCityRequest, City>();
            CreateMap<UpdateCityRequest, City>();

            CreateMap<City, CityResponse>();
            CreateMap<Page<City>, Page<CityResponse>>();

            CreateMap<CitySummary, CitySummaryResponse>();
            CreateMap<City, CitySummary>();

            //Country
            CreateMap<Country, CountryResponse>();

            //Reports
            CreateMap<Event, EventReportResponse>().ConvertUsing<EventReportTypeConverter>();
            CreateMap<EventPackage, PackageOverviewReportResponse>()
                .ForMember(c => c.PackageName, opt => opt.MapFrom(src => src.Title))
                .ForMember(c => c.NetPrice, opt => opt.MapFrom(src => src.Price));

            CreateMap<EventBooking, EventBookingDetailedReportResponse>()
                .ForMember(c => c.EventId, opt => opt.MapFrom(src => src.Event.Id))
                .ForMember(c => c.EventName, opt => opt.MapFrom(src => src.Event.Name))
                .ForMember(c => c.Occurrence, opt => opt.MapFrom(src => src.Event.Occurrence))
                .ForMember(c => c.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate/*.Add(new TimeSpan(_appSettings.TimeZoneOffset, 0, 0))*/))
                .ForMember(c => c.Attendees, opt => opt.ConvertUsing<AttendeesCountPerEventBookingValueConverter, EventBooking>(src => src))
                .ForMember(c => c.Absentees, opt => opt.ConvertUsing<AbsenteesCountPerEventBookingValueConverter, EventBooking>(src => src));
            CreateMap<Page<EventBooking>, Page<EventBookingDetailedReportResponse>>();

            CreateMap<EventBooking, EventBookingDetailedReportDTO>()
                .ForMember(c => c.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(c => c.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(c => c.Price, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(c => c.CreationDate, opt => opt.MapFrom(src => src.CreatedDate/*.Add(new TimeSpan(_appSettings.TimeZoneOffset, 0, 0))*/.ToString("dd MMM yyyy")))
                .ForMember(c => c.ReservationDate, opt => opt.MapFrom(src => src.Event.Occurrence.GetStartDateTime().ToString("dd MMM yyyy")))
                .ForMember(c => c.Attendees, opt => opt.ConvertUsing<AttendeesCountPerEventBookingValueConverter, EventBooking>(src => src))
                .ForMember(c => c.Absentees, opt => opt.ConvertUsing<AbsenteesCountPerEventBookingValueConverter, EventBooking>(src => src));

            CreateMap<Venue, VenueReportResponse>().ConvertUsing<VenueReportTypeConverter>();

            CreateMap<VenueOneOffer, OfferReportResponse>()
                .ForMember(c => c.Offer, opt => opt.MapFrom(src => src.Offer.Type.Name))
                .ForMember(c => c.VenueCity, opt => opt.MapFrom(src => src.Location != null ? src.Location.City.Name : null))
                .ForMember(c => c.TotalOfferUsage, opt => opt.ConvertUsing<OfferTypeUsageCountPerVenueValueConverter, VenueOneOffer>(src => src));
            CreateMap<Page<VenueOneOffer>, Page<OfferReportResponse>>();

            CreateMap<VenueBooking, VenueBookingDetailedReportResponse>()
                .ForMember(c => c.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate/*.Add(new TimeSpan(_appSettings.TimeZoneOffset, 0, 0))*/))
                .ForMember(c => c.ReservationDate, opt => opt.MapFrom(src => src.Date))
                .ForPath(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToDisplayName()))
                .ForMember(c => c.TotalReservations, opt => opt.MapFrom(src => src.PeopleNumber));
            CreateMap<Page<VenueBooking>, Page<VenueBookingDetailedReportResponse>>();

            CreateMap<VenueBooking, VenueBookingDetailedReportDTO>()
                .ForMember(c => c.CustomerName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(c => c.Status, opt => opt.MapFrom(src => src.Status.ToDisplayName()))
                .ForMember(c => c.OrderNumber, opt => opt.MapFrom(src => src.BookingNumber))
                .ForMember(c => c.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(c => c.TotalReservations, opt => opt.MapFrom(src => src.PeopleNumber))
                .ForMember(c => c.CreationDate, opt => opt.MapFrom(src => src.CreatedDate/*.Add(new TimeSpan(_appSettings.TimeZoneOffset, 0, 0))*/.ToString("dd MMM yyyy")))
                .ForMember(c => c.ReservationDate, opt => opt.MapFrom(src => src.Date.ToString("dd MMM yyyy")))
                .ForMember(c => c.ReservationTime, opt => opt.MapFrom(src => src.Date.ToString("h:mm tt")));
        }

        private string To12HrFormat(TimeSpan time)
        {
            DateTime date = DateTime.Today.Add(time);
            return date.ToString("HH:mm");
        }
        private string To12HrFullFormat(TimeSpan time)
        {
            DateTime date = DateTime.Today.Add(time);
            return date.ToString("h:mm tt");
        }
        private EventOccurrence GetNearestOccurrenceInDate(List<EventOccurrence> occurrences)
        {
            var newEvents = occurrences.Where(a => a.GetStartDateTime() >= UAEDateTime.Now).OrderByDescending(a => a.GetStartDateTime()).ToList();
            var oldEvents = occurrences.Where(a => a.GetStartDateTime() < UAEDateTime.Now).OrderByDescending(a => a.GetStartDateTime()).ToList();
            return newEvents.Any() ? newEvents.LastOrDefault() : oldEvents.FirstOrDefault();
        }
        private DateTime GetNextDayOccurance(DateTime from, List<AvailableTime> availableTimes)
        {
            List<DateTime> availableDates = new List<DateTime>();
            var start = from.DayOfWeek;
            foreach (var availableTime in availableTimes)
            {
                foreach (var day in availableTime.Days)
                {
                    var date = new DateTime(from.Year, from.Month, from.Day, availableTime.From.Hours, availableTime.From.Minutes, availableTime.From.Seconds);
                    if (day < start)
                    {
                        var diff = (int)day - (int)start;
                        availableDates.Add(date.AddDays(diff + 7));
                    }
                    else if (day == start)
                    {
                        if (from.TimeOfDay < availableTime.From)
                            availableDates.Add(date);
                        else
                            availableDates.Add(date.AddDays(7));
                    }
                    else
                    {
                        var diff = (int)day - (int)start;
                        availableDates.Add(date.AddDays(diff));
                    }
                }
            }

            var result = availableDates.OrderBy(d => d).FirstOrDefault();
            return result;
        }
    }
}
