using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOut.Constants.Enums;
using OutOut.Core.Services;
using OutOut.Infrastructure.Services;
using OutOut.Persistence.Interfaces;

namespace OutOut.Core.BackgroundServices
{
    public class UnholdEventTicketsService : BackgroundService
    {
        private readonly IEventBookingRepository _eventBookingRepository;
        private readonly EventBookingService _eventBookingService;
        private readonly PaymentService _paymentService;

        public UnholdEventTicketsService(IServiceScopeFactory services)
        {
            var sp = services.CreateScope().ServiceProvider;
            _eventBookingRepository = sp.GetRequiredService<IEventBookingRepository>();
            _eventBookingService = sp.GetRequiredService<EventBookingService>();
            _paymentService = sp.GetRequiredService<PaymentService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var stalePendingBookings = await _eventBookingRepository.GetStalePendingBooking();
                foreach (var booking in stalePendingBookings)
                {
                    var response = await _paymentService.CheckTelrTransaction(booking.OrderReference);

                    if (response?.Order?.Status?.Text == PaymentStatus.Pending.ToString())
                        continue;

                    await _eventBookingService.HandleTelrBooking(booking.Id);
                }
                await Task.Delay(15 * 60 * 1000);
            }
        }
    }
}
