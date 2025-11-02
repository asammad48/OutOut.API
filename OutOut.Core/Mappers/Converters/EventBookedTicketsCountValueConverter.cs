using AutoMapper;
using OutOut.Models.Domains;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class EventBookedTicketsCountValueConverter : IValueConverter<Event, long>, IValueConverter<SingleEventOccurrence, long>
    {
        private readonly IEventBookingRepository _eventBookingRepository;

        public EventBookedTicketsCountValueConverter(IEventBookingRepository eventBookingRepository)
        {
            _eventBookingRepository = eventBookingRepository;
        }
        public long Convert(Event eventObject, ResolutionContext context)
        {
            return _eventBookingRepository.GetPaidTicketsCount(eventObject.Id);
        }
        public long Convert(SingleEventOccurrence eventObject, ResolutionContext context)
        {
            return _eventBookingRepository.GetPaidTicketsCount(eventObject.Id, eventObject.Occurrence.Id);
        }
    }
}
