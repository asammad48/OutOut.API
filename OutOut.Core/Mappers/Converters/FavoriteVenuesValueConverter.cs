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
            if (_userDetailsProvider.User == null)
            {
                return false;
            }
            return _userDetailsProvider.User.FavoriteVenues.Contains(venueId);
        }
    }
}
