using AutoMapper;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class CustomerLoyaltyRedemptionCountValueConverter : IValueConverter<UserLoyalty, long>
    {
        private readonly IUserLoyaltyRepository _userLoyaltyRepository;

        public CustomerLoyaltyRedemptionCountValueConverter(IUserLoyaltyRepository userLoyaltyRepository)
        {
            _userLoyaltyRepository = userLoyaltyRepository;
        }
        public long Convert(UserLoyalty userLoyalty, ResolutionContext context)
        {
            return _userLoyaltyRepository.GetRedemptionsCount(userLoyalty.UserId, userLoyalty.Venue.Id);
        }
    }
}
