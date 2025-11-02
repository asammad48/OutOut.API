using AutoMapper;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class CustomerOfferRedemptionCountValueConverter : IValueConverter<UserOffer, long>
    {
        private readonly IUserOfferRepository _userOfferRepository;

        public CustomerOfferRedemptionCountValueConverter(IUserOfferRepository userOfferRepository)
        {
            _userOfferRepository = userOfferRepository;
        }
        public long Convert(UserOffer userOffer, ResolutionContext context)
        {
            return _userOfferRepository.GetRedemptionsCount(userOffer.Offer.Type.Id, userOffer.UserId, userOffer.Venue.Id);
        }
    }
}
