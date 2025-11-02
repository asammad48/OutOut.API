using AutoMapper;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class LoyaltyTypeUsageCountValueConverter : IValueConverter<string, long>
    {
        private readonly IUserLoyaltyRepository _userLoyaltyRepository;

        public LoyaltyTypeUsageCountValueConverter(IUserLoyaltyRepository userLoyaltyRepository)
        {
            _userLoyaltyRepository = userLoyaltyRepository;
        }
        public long Convert(string loyaltyTypeId, ResolutionContext context)
        {
            return _userLoyaltyRepository.GetConsumedUserLoyaltyCount(loyaltyTypeId);
        }
    }
}
