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
            var user = _userDetailsProvider.User;
            return user.FavoriteEvents.Contains(eventOccurrenceId);
        }
    }
}
