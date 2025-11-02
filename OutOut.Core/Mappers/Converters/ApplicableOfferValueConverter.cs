using AutoMapper;
using OutOut.Core.Utils;
using OutOut.Models.Models;
using OutOut.Models.Utils;

namespace OutOut.Core.Mappers.Converters
{
    public class ApplicableOfferValueConverter : IValueConverter<UserOffer, bool>, IValueConverter<Offer, bool>
    {
        public bool Convert(UserOffer userOffer, ResolutionContext context)
        {
            var isInRange = userOffer.Offer.ValidOn.IsInRangeOf(UAEDateTime.Now);

            if (userOffer == null)
                return isInRange;

            return !userOffer.HasReachedLimit && isInRange;
        }

        public bool Convert(Offer offer, ResolutionContext context)
        {
            return offer.ValidOn.IsInRangeOf(UAEDateTime.Now);
        }
    }
}
