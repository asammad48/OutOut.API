using AutoMapper;
using OutOut.Models.Models;

namespace OutOut.Core.Mappers.Converters
{
    public class EventRemainingTicketsCountValueConverter : IValueConverter<Event, long>
    {
        public long Convert(Event eventObject, ResolutionContext context)
        {
            return eventObject.Occurrences.SelectMany(a => a.Packages).Sum(a => (long)a.RemainingTickets);
        }
    }
}
