using AutoMapper;
using OutOut.Models.Models;
using OutOut.Models.Utils;
using OutOut.Persistence.Interfaces;
using OutOut.ViewModels.Responses.Events;
using System.Linq;

namespace OutOut.Core.Mappers.Converters
{
    public class EventReportTypeConverter : ITypeConverter<Event, EventReportResponse>
    {
        private readonly IEventBookingRepository _eventBookingRepository;

        public EventReportTypeConverter(IEventBookingRepository eventBookingRepository)
        {
            _eventBookingRepository = eventBookingRepository;
        }

        public EventReportResponse Convert(Event existingEvent, EventReportResponse eventResponse, ResolutionContext context)
        {
            return new EventReportResponse
            {
                Id = existingEvent.Id,
                Name = existingEvent.Name,
                IsEnded = existingEvent.Occurrences.OrderBy(a => a.GetStartDateTime()).LastOrDefault().GetStartDateTime() <= UAEDateTime.Now,
                TicketsBooked = _eventBookingRepository.GetPaidTicketsCount(existingEvent.Id),
                TicketsLeft = existingEvent.Occurrences.SelectMany(a => a.Packages).Sum(a => a.RemainingTickets),
                Revenue = _eventBookingRepository.GetRevenueForEvent(existingEvent.Id),
                Attendees = _eventBookingRepository.GetAttendeesCountForEvent(existingEvent.Id),
                Absentees = _eventBookingRepository.GetAbsenteesCountForEvent(existingEvent.Id)
            };
        }
    }
}
