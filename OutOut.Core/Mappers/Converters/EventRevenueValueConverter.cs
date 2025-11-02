using AutoMapper;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class EventRevenueValueConverter : IValueConverter<string, double>
    {
        private readonly IEventBookingRepository _eventBookingRepository;
        public EventRevenueValueConverter(IEventBookingRepository eventBookingRepository)
        {
            _eventBookingRepository = eventBookingRepository;
        }
        public double Convert(string eventId, ResolutionContext context)
        {
            return _eventBookingRepository.GetRevenueForEvent(eventId);
        }
    }
}
