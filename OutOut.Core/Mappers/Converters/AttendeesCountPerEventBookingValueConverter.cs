using AutoMapper;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.Mappers.Converters
{
    public class AttendeesCountPerEventBookingValueConverter : IValueConverter<EventBooking, long>
    {
        private readonly IEventBookingRepository _eventBookingRepository;

        public AttendeesCountPerEventBookingValueConverter(IEventBookingRepository eventBookingRepository)
        {
            _eventBookingRepository = eventBookingRepository;
        }

        public long Convert(EventBooking eventBooking, ResolutionContext context)
        {
            return _eventBookingRepository.GetAttendeesCountForBooking(eventBooking.Id);
        }
    }
}
