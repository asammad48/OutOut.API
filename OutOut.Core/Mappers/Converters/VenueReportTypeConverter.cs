using AutoMapper;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;
using OutOut.ViewModels.Responses.Venues;

namespace OutOut.Core.Mappers.Converters
{
    public class VenueReportTypeConverter : ITypeConverter<Venue, VenueReportResponse>
    {
        private readonly IVenueBookingRepository _venueBookingRepository;
        private readonly IUserLoyaltyRepository _userLoyaltyRepository;

        public VenueReportTypeConverter(IVenueBookingRepository venueBookingRepository, IUserLoyaltyRepository userLoyaltyRepository)
        {
            _venueBookingRepository = venueBookingRepository;
            _userLoyaltyRepository = userLoyaltyRepository;
        }

        public VenueReportResponse Convert(Venue venue, VenueReportResponse venueReportResponse, ResolutionContext context)
        {
            return new VenueReportResponse
            {
                Id = venue.Id,
                Name = venue.Name,
                TotalBookingsCount = _venueBookingRepository.GetAllBookingsCount(venue.Id),
                ApprovedBookingsCount = _venueBookingRepository.GetApprovedBookingsCount(venue.Id),
                CancelledBookingsCount = _venueBookingRepository.GetCancelledBookingsCount(venue.Id),
                LoyaltyUsageCount = _userLoyaltyRepository.GetLoyaltyUsageCount(venue.Id)
            };
        }
    }
}
