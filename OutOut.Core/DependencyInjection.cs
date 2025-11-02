using Microsoft.Extensions.DependencyInjection;
using OutOut.Core.Mappers;
using OutOut.Core.Mappers.Converters;
using OutOut.Core.Services;
using OutOut.Core.Utils;
using OutOut.Models;
using OutOut.Models.Identity;
using OutOut.Persistence.Identity;

namespace OutOut.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAutomapper(this IServiceCollection services, AppSettings appSettings)
        {
            var serviceProvider = services.BuildServiceProvider();

            services.AddScoped<LocationTypeConverter>();
            services.AddScoped<EventReportTypeConverter>();
            services.AddScoped<VenueReportTypeConverter>();

            services.AddScoped<FavoriteVenuesValueConverter>();
            services.AddScoped<FavoriteEventsValueConverter>();
            services.AddScoped<ApplicableLoyaltyValueConverter>();
            services.AddScoped<ApplicableOfferValueConverter>();
            services.AddScoped<VenueApprovedBookingsCountValueConverter>();
            services.AddScoped<EventBookedTicketsCountValueConverter>();
            services.AddScoped<EventPendingTicketsCountValueConverter>();
            services.AddScoped<EventRemainingTicketsCountValueConverter>();
            services.AddScoped<EventRevenueValueConverter>();
            services.AddScoped<OfferUsageCountValueConverter>();
            services.AddScoped<OfferTypeUsageCountValueConverter>();
            services.AddScoped<OfferTypeAssignmentCountValueConverter>();
            services.AddScoped<LoyaltyTypeAssignmentCountValueConverter>();
            services.AddScoped<LoyaltyTypeUsageCountValueConverter>();
            services.AddScoped<CustomerLoyaltyRedemptionCountValueConverter>();
            services.AddScoped<CustomerOfferRedemptionCountValueConverter>();
            services.AddScoped<AbsenteesCountPerEventBookingValueConverter>();
            services.AddScoped<AttendeesCountPerEventBookingValueConverter>();
            services.AddScoped<OfferTypeUsageCountPerVenueValueConverter>();
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile(new MappingProfile(appSettings));
                cfg.ConstructServicesUsing(type => ActivatorUtilities.CreateInstance(serviceProvider, type));
            });

            return services;
        }

        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddScoped<AuthService>();
            services.AddScoped<CustomerService>();
            services.AddScoped<AdminProfileService>();
            services.AddScoped<LocationService>();
            services.AddScoped<CustomerSupportService>();
            services.AddScoped<RefreshTokensFactory<ApplicationUser>>();
            services.AddScoped<FAQService>();
            services.AddScoped<TermsAndConditionsService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<VenueService>();
            services.AddScoped<VenueBookingService>();
            services.AddScoped<VenueRequestService>();
            services.AddScoped<EventService>();
            services.AddScoped<EventBookingService>();
            services.AddScoped<EventRequestService>();
            services.AddScoped<LoyaltyService>();
            services.AddScoped<OfferService>();
            services.AddScoped<HomeScreenService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<CityService>();
            services.AddScoped<BookingService>();
            
            services.AddScoped<ITimeProvider, DefaultTimeProvider>();

            return services;
        }
    }
}
