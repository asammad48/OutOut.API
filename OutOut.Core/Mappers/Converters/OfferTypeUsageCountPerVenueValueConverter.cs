using AutoMapper;
using OutOut.Models.Domain;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class OfferTypeUsageCountPerVenueValueConverter : IValueConverter<VenueOneOffer, long>
    {
        private readonly IUserOfferRepository _userOfferRepository;
        public OfferTypeUsageCountPerVenueValueConverter(IUserOfferRepository userOfferRepository)
        {
            _userOfferRepository = userOfferRepository;
        }
        public long Convert(VenueOneOffer venueOneOffer, ResolutionContext context)
        {
            return _userOfferRepository.GetOfferUsageCountByVenueId(venueOneOffer.Id, venueOneOffer.Offer.Type.Id);
        }
    }
}
