using AutoMapper;
using OutOut.Persistence.Providers;

namespace OutOut.Core.Mappers.Converters
{
    public class FavoriteEventsValueConverter : IValueConverter<string, bool>
    {
        public readonly IUserDetailsProvider _userDetailsProvider;

        public FavoriteEventsValueConverter(IUserDetailsProvider userDetailsProvider)
        {
            _userDetailsProvider = userDetailsProvider;
        }

        public bool Convert(string eventOccurrenceId, ResolutionContext context)
        {
            if (_userDetailsProvider.User == null)
            {
                return false;
            }
            return _userDetailsProvider.User.FavoriteEvents.Contains(eventOccurrenceId);
        }
    }
}
