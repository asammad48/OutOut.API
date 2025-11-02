using AutoMapper;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class EventPendingTicketsCountValueConverter : IValueConverter<Event, long>
    {
        private readonly IEventBookingRepository _eventBookingRepository;

        public EventPendingTicketsCountValueConverter(IEventBookingRepository eventBookingRepository)
        {
            _eventBookingRepository = eventBookingRepository;
        }
        public long Convert(Event eventObject, ResolutionContext context)
        {
            return _eventBookingRepository.GetPendingTicketsCount(eventObject.Id);
        }
    }
}
