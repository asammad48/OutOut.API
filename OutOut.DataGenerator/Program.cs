using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOut.DataGenerator.Editors;
using OutOut.DataGenerator.Migrators;
using OutOut.Models.Exceptions;
using Serilog;

namespace OutOut.DataGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            ConfigureLogger();

            var venueGenerator = scope.ServiceProvider.GetRequiredService<VenueGenerator>();
            var offersGenerator = scope.ServiceProvider.GetRequiredService<OffersGenerator>();
            var loyatyGenerator = scope.ServiceProvider.GetRequiredService<LoyaltyGenerator>();
            var eventsGenerator = scope.ServiceProvider.GetRequiredService<EventsGenerator>();
            var venueEditor = scope.ServiceProvider.GetRequiredService<VenueEditor>();
            var eventsEditor = scope.ServiceProvider.GetRequiredService<EventsEditor>();
            var offersEditor = scope.ServiceProvider.GetRequiredService<OffersEditor>();
            var offersTypeMigrator = scope.ServiceProvider.GetRequiredService<OfferTypesMigrator>();

            await venueGenerator.Generate();
            await offersGenerator.Generate();
            await loyatyGenerator.Generate();
            //await eventsGenerator.Generate();
            //await eventsGenerator.GenerateOccurences();

            //await offersTypeMigrator.MigrateVenues();
            //await venueEditor.ModifyDescription();
            //await eventsEditor.ModifyPackages();
           // await offersEditor.ModifyOffers();

            Console.WriteLine("");
            Console.WriteLine("All Generators ran successfully..");
            Console.ReadLine();
        }

        private static void ConfigureLogger()
        {
            var config = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Filter.ByExcluding(logEvent =>
                {
                    return logEvent.Exception != null &&
                    (logEvent.Exception.GetType() == typeof(OutOutException));
                });

            Log.Logger = config.WriteTo.Console()
                               .WriteTo.File("Logs/selfservices_updater-.txt", rollingInterval: RollingInterval.Day)
                               .CreateLogger();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://localhost:5000");
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddScoped<VenueGenerator>();
                    services.AddScoped<OffersGenerator>();
                    services.AddScoped<LoyaltyGenerator>();
                    services.AddScoped<EventsGenerator>();
                    services.AddScoped<VenueEditor>();
                    services.AddScoped<EventsEditor>();
                    services.AddScoped<OffersEditor>();
                    services.AddScoped<OfferTypesMigrator>();
                });
    }
}
