using AutoMapper;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class LoyaltyTypeAssignmentCountValueConverter : IValueConverter<string, long>
    {
        private readonly IVenueRepository _venueRepository;

        public LoyaltyTypeAssignmentCountValueConverter(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }
        public long Convert(string loyaltyTypeId, ResolutionContext context)
        {
            return _venueRepository.GetAssignedLoyaltyCount(loyaltyTypeId);
        }
    }
}
