using AutoMapper;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class OfferUsageCountValueConverter : IValueConverter<string, long>
    {
        private readonly IUserOfferRepository _userOfferRepository;
        public OfferUsageCountValueConverter(IUserOfferRepository userOfferRepository)
        {
            _userOfferRepository = userOfferRepository;
        }
        public long Convert(string offerId, ResolutionContext context)
        {
            return _userOfferRepository.GetUserOffersCountById(offerId);
        }
    }
}
