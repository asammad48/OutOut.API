using AutoMapper;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class OfferTypeAssignmentCountValueConverter : IValueConverter<string, long>
    {
        private readonly IOfferRepository _offerRepository;
        public OfferTypeAssignmentCountValueConverter(IOfferRepository offerRepository)
        {
            _offerRepository = offerRepository;
        }
        public long Convert(string offerTypeId, ResolutionContext context)
        {
            return _offerRepository.GetAssignedOffersCount(offerTypeId);
        }
    }
}
