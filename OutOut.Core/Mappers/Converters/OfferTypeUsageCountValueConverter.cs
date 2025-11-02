using AutoMapper;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class OfferTypeUsageCountValueConverter : IValueConverter<string, long>
    {
        private readonly IUserOfferRepository _userOfferRepository;
        public OfferTypeUsageCountValueConverter(IUserOfferRepository userOfferRepository)
        {
            _userOfferRepository = userOfferRepository;
        }
        public long Convert(string offerTypeId, ResolutionContext context)
        {
            return _userOfferRepository.GetUserOffersCount(offerTypeId);
        }
    }
}
