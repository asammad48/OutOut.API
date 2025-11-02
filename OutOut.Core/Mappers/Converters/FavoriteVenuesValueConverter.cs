using AutoMapper;
using OutOut.Persistence.Providers;

namespace OutOut.Core.Mappers.Converters
{
    public class FavoriteVenuesValueConverter : IValueConverter<string, bool>
    {
        public readonly IUserDetailsProvider _userDetailsProvider;

        public FavoriteVenuesValueConverter(IUserDetailsProvider userDetailsProvider)
        {
            _userDetailsProvider = userDetailsProvider;
        }

        public bool Convert(string venueId, ResolutionContext context)
        {
            var user = _userDetailsProvider.User;
            return user.FavoriteVenues.Contains(venueId);
        }
    }
}
