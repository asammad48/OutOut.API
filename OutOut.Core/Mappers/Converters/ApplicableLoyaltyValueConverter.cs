using AutoMapper;
using OutOut.Constants.Enums;
using OutOut.Core.Services;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class ApplicableLoyaltyValueConverter : IValueConverter<UserLoyalty, bool>, IValueConverter<Loyalty, bool>
    {
        private readonly LoyaltyService _loyaltyService;
        private readonly IVenueRepository _venueRepository;

        public ApplicableLoyaltyValueConverter(LoyaltyService loyaltyService, IVenueRepository venueRepository)
        {
            _loyaltyService = loyaltyService;
            _venueRepository = venueRepository;
        }

        public bool Convert(UserLoyalty userLoyalty, ResolutionContext context)
        {
            return _loyaltyService.IsLoyaltyApplicable(userLoyalty.Loyalty, LoyaltyService.ExtractRedeemsToday(userLoyalty), userLoyalty.CanGet)
                && _venueRepository.GetById(userLoyalty?.Venue?.Id).Result?.Status == Availability.Active;
        }

        public bool Convert(Loyalty loyalty, ResolutionContext context)
        {
            return _loyaltyService.IsLoyaltyApplicable(loyalty) && _venueRepository.GetByLoyaltyId(loyalty?.Id).Result?.Status == Availability.Active;
        }
    }
}
