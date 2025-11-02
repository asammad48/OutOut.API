using AutoMapper;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class VenueApprovedBookingsCountValueConverter : IValueConverter<string, long>
    {
        private readonly IVenueBookingRepository _venueBookingRepository;
        public VenueApprovedBookingsCountValueConverter(IVenueBookingRepository venueBookingRepository)
        {
            _venueBookingRepository = venueBookingRepository;
        }
        public long Convert(string venueId, ResolutionContext context)
        {
            return _venueBookingRepository.GetApprovedBookingsCount(venueId);
        }
    }
}
