using Microsoft.Extensions.DependencyInjection;
using OutOut.Models;
using OutOut.Persistence.Data;
using OutOut.Persistence.Interfaces;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.Persistence.Providers;
using OutOut.Persistence.Services;
using System.Reflection;
using Type = System.Type;
namespace OutOut.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddScoped<ApplicationNonSqlDbContext>();

            // Providers Registration
            services.AddScoped<IUserDetailsProvider, UserDetailsProvider>();

            // Repos Registeration
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserLocationRepository, UserLocationRepository>();
            services.AddScoped<IApplicationStateRepository, ApplicationStateRepository>();
            services.AddScoped<IFAQRepository, FAQRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITermsAndConditionsRepository, TermsAndConditionsRepository>();
            services.AddScoped<ICustomerSupportRepository, CustomerSupportRepository>();
            services.AddScoped<IVenueRepository, VenueRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventRequestRepository, EventRequestRepository>();
            services.AddScoped<IVenueBookingRepository, VenueBookingRepository>();
            services.AddScoped<IVenueRequestRepository, VenueRequestRepository>();
            services.AddScoped<IEventBookingRepository, EventBookingRepository>();
            services.AddScoped<IUserLoyaltyRepository, UserLoyaltyRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IOfferRepository, OfferRepository>();
            services.AddScoped<IUserOfferRepository, UserOfferRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IOfferTypeRepository, OfferTypeRepository>();
            services.AddScoped<ILoyaltyTypeRepository, LoyaltyTypeRepository>();
            
            // Sync Repositories
            services.AddSyncRepositories();

            return services;
        }

        private static IServiceCollection AddSyncRepositories(this IServiceCollection services)
        {
            var types = typeof(DependencyInjection).Assembly.DefinedTypes;
            List<TypeInfo> syncRepositoriesTypeInfos = types.Where(t => IsAssignableToGenericType(t, typeof(ISyncRepository<>)) && !t.IsInterface).ToList();

            foreach (var syncRepositoryTypeInfo in syncRepositoriesTypeInfos)
            {
                foreach (var implementedInterface in syncRepositoryTypeInfo.ImplementedInterfaces)
                {
                    services.AddScoped(implementedInterface, syncRepositoryTypeInfo);
                }
            }

            return services;
        }

        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }
    }
}
