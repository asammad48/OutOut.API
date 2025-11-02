using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using OutOut.Constants;
using OutOut.Models.Identity;
using OutOut.Models.Models;
using OutOut.Persistence.Data;
using OutOut.Constants.Enums;
using OutOut.Persistence.Interfaces;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Bson;
using OutOut.Models;
using Microsoft.Extensions.Options;
using OutOut.Models.Models.Embedded;
using Newtonsoft.Json;

namespace OutOut.Helpers
{
    public class DatabaseSeeder
    {
        private readonly IWebHostEnvironment _environment;
        private readonly AppSettings _appSettings;
        public DatabaseSeeder(IWebHostEnvironment webHostEnvironment, IOptions<AppSettings> appSettings)
        {
            _environment = webHostEnvironment;
            _appSettings = appSettings.Value;
        }

        public async Task Initialize(IServiceProvider serviceProvider, ApplicationNonSqlDbContext nonSqlDbContext)
        {
            //return;
            await SeedSuperAdmin(serviceProvider);
            SeedIndexes(nonSqlDbContext);
            await SeedTermsAndConditions(serviceProvider);
            await SeedLoyaltyTypes(serviceProvider);
            await SeedOfferTypes(serviceProvider);
            await SeedCategories(serviceProvider);
            await SeedCountries(serviceProvider);
            await SeedCities(serviceProvider);
            await SeedEvents(serviceProvider);
            await SeedVenues(serviceProvider);
            await AddVenuesToEvents(serviceProvider);
            await SeedFAQ(serviceProvider);
        }

        public async Task SeedSuperAdmin(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            bool superAdminRoleExists = await roleManager.RoleExistsAsync(Roles.SuperAdmin);
            if (!superAdminRoleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole(Roles.SuperAdmin));
            }
            bool venueAdminRoleExists = await roleManager.RoleExistsAsync(Roles.VenueAdmin);
            if (!venueAdminRoleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole(Roles.VenueAdmin));
            }
            bool eventAdminRoleExists = await roleManager.RoleExistsAsync(Roles.EventAdmin);
            if (!eventAdminRoleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole(Roles.EventAdmin));
            }
            bool ticketAdminRoleExists = await roleManager.RoleExistsAsync(Roles.TicketAdmin);
            if (!ticketAdminRoleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole(Roles.TicketAdmin));
            }

            var superAdminEmail = _appSettings.SuperAdminEmail;
            var superAdminFullName = "Super Admin";

            ApplicationUser superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdminUser == null)
            {
                await userManager.CreateAsync(new ApplicationUser
                {
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    UserName = Guid.NewGuid().ToString(),
                    FullName = superAdminFullName,
                    Location = new UserLocation(55.2744, 25.1972, "Burj Khalifa")
                }, "Test@123");

                superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
            }

            var rolesList = await userManager.GetRolesAsync(superAdminUser);
            if (!rolesList.Contains(Roles.SuperAdmin))
            {
                await userManager.AddToRoleAsync(superAdminUser, Roles.SuperAdmin);
            }

            return;
        }

        public void SeedIndexes(ApplicationNonSqlDbContext nonSqlDbContext)
        {
            //ApplicationState unique key index
            IMongoCollection<ApplicationState> applicationStateCollection = nonSqlDbContext.GetCollection<ApplicationState>();
            var ApplicationStateKeyUniqeIndexModel = new CreateIndexModel<ApplicationState>(
                new IndexKeysDefinitionBuilder<ApplicationState>().Ascending(c => c.Key), new CreateIndexOptions { Unique = true }
                );
            applicationStateCollection.Indexes.DropAll();
            applicationStateCollection.Indexes.CreateMany(new List<CreateIndexModel<ApplicationState>> { ApplicationStateKeyUniqeIndexModel });

            //Create Geo2dSphere index for location
            IMongoCollection<ApplicationUser> applicationUserCollection = nonSqlDbContext.GetCollection<ApplicationUser>();
            var ApplicationUserGeoSpatialIndexModel = new CreateIndexModel<ApplicationUser>(
                Builders<ApplicationUser>.IndexKeys.Geo2DSphere(it => it.Location.GeoPoint)
                );
            applicationUserCollection.Indexes.DropAll();
            applicationUserCollection.Indexes.CreateMany(new List<CreateIndexModel<ApplicationUser>> { ApplicationUserGeoSpatialIndexModel });

            //Create Geo2dSphere index for venue location
            IMongoCollection<Venue> venueCollection = nonSqlDbContext.GetCollection<Venue>();
            var VenueGeoSpatialIndexModel = new CreateIndexModel<Venue>(
                Builders<Venue>.IndexKeys.Geo2DSphere(it => it.Location.GeoPoint)
                );
            venueCollection.Indexes.DropAll();
            venueCollection.Indexes.CreateMany(new List<CreateIndexModel<Venue>> { VenueGeoSpatialIndexModel });

            //Create Geo2dSphere index for event location
            IMongoCollection<Event> eventCollection = nonSqlDbContext.GetCollection<Event>();
            var EventGeoSpatialIndexModel = new CreateIndexModel<Event>(
                Builders<Event>.IndexKeys.Geo2DSphere(it => it.Location.GeoPoint)
                );
            eventCollection.Indexes.DropAll();
            eventCollection.Indexes.CreateMany(new List<CreateIndexModel<Event>> { EventGeoSpatialIndexModel });

            //Terms & Conditions unique index
            IMongoCollection<TermsAndConditions> termsAndConditionsCollection = nonSqlDbContext.GetCollection<TermsAndConditions>();
            var TermsAndConditionsUniqeIndexModel = new CreateIndexModel<TermsAndConditions>(
                new IndexKeysDefinitionBuilder<TermsAndConditions>().Ascending(c => c.TermCondition), new CreateIndexOptions { Unique = true });
            termsAndConditionsCollection.Indexes.DropAll();
            termsAndConditionsCollection.Indexes.CreateMany(new List<CreateIndexModel<TermsAndConditions>> { TermsAndConditionsUniqeIndexModel });

            //VenueBooking unique booking number index
            IMongoCollection<VenueBooking> venueBookingCollection = nonSqlDbContext.GetCollection<VenueBooking>();
            var venueBookingNumberUniqeIndexModel = new CreateIndexModel<VenueBooking>(
                new IndexKeysDefinitionBuilder<VenueBooking>().Ascending(c => c.BookingNumber), new CreateIndexOptions { Unique = true });
            venueBookingCollection.Indexes.DropAll();
            venueBookingCollection.Indexes.CreateMany(new List<CreateIndexModel<VenueBooking>> { venueBookingNumberUniqeIndexModel });

            //City unique name index
            IMongoCollection<City> cityCollection = nonSqlDbContext.GetCollection<City>();
            var cityNameUniqeIndexModel = new CreateIndexModel<City>(
                new IndexKeysDefinitionBuilder<City>().Ascending(c => c.Name), new CreateIndexOptions { Unique = true });
            cityCollection.Indexes.DropAll();
            cityCollection.Indexes.CreateMany(new List<CreateIndexModel<City>> { cityNameUniqeIndexModel });

            //EventBooking unique order number index
            IMongoCollection<EventBooking> eventBookingCollection = nonSqlDbContext.GetCollection<EventBooking>();
            var eventBookingOrderNumberUniqeIndexModel = new CreateIndexModel<EventBooking>(
                new IndexKeysDefinitionBuilder<EventBooking>().Ascending(c => c.OrderNumber), new CreateIndexOptions { Unique = true });
            eventBookingCollection.Indexes.DropAll();
            eventBookingCollection.Indexes.CreateMany(new List<CreateIndexModel<EventBooking>> { eventBookingOrderNumberUniqeIndexModel });

            //VenueRequest unique index
            //IMongoCollection<VenueRequest> venueRequestCollection = nonSqlDbContext.GetCollection<VenueRequest>();
            //var VenueRequestUniqeIndexModel = new CreateIndexModel<VenueRequest>(
            //    new IndexKeysDefinitionBuilder<VenueRequest>().Ascending(a => a.Venue.Id).Ascending(a => a.LastModificationRequest.Type),
            //    new CreateIndexOptions { Unique = true }
            //    );
            //venueRequestCollection.Indexes.DropAll();
            //venueRequestCollection.Indexes.CreateMany(new List<CreateIndexModel<VenueRequest>> { VenueRequestUniqeIndexModel });

            //EventRequest unique index
            //IMongoCollection<EventRequest> eventRequestCollection = nonSqlDbContext.GetCollection<EventRequest>();
            //var EventRequestUniqeIndexModel = new CreateIndexModel<EventRequest>(
            //    new IndexKeysDefinitionBuilder<EventRequest>().Ascending(a => a.Event.Id).Ascending(a => a.LastModificationRequest.Type),
            //    new CreateIndexOptions { Unique = true }
            //    );
            //eventRequestCollection.Indexes.DropAll();
            //eventRequestCollection.Indexes.CreateMany(new List<CreateIndexModel<EventRequest>> { EventRequestUniqeIndexModel });

            return;
        }

        public async Task SeedCountries(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<ICountryRepository>();
            var res = await service.GetAll();
            if (res.Any())
                return;

            var folderDetails = Path.Combine(Directory.GetCurrentDirectory(), _environment.WebRootPath, _appSettings.CountriesFileName);
            var JSON = File.ReadAllText(folderDetails);
            dynamic jsonObj = JsonConvert.DeserializeObject(JSON);

            foreach (var country in jsonObj)
            {
                var newCountry = new Country
                {
                    Name = country["Name"].ToString(),
                    Symbol = country["Symbol"].ToString()
                };
                await service.Create(newCountry);
            }
        }
        public async Task SeedCities(IServiceProvider serviceProvider)
        {
            var countryService = serviceProvider.GetRequiredService<ICountryRepository>();
            var UAE = (await countryService.GetAll()).Where(a => a.Name == "United Arab Emirates").FirstOrDefault();

            var service = serviceProvider.GetRequiredService<ICityRepository>();
            var res = await service.GetAll();
            if (res.Any())
                return;

            var dubai = new City
            {
                Name = "Dubai",
                Areas = new List<string> { Areas.BurjKhalifa, Areas.DIFC, Areas.DubaiCreek },
                Country = UAE
            };
            var abuDhabi = new City
            {
                Name = "Abu Dhbai",
                Areas = new List<string> { Areas.AlZahya, Areas.YasIslands, Areas.KhalifaCity },
                Country = UAE
            };
            var ajman = new City
            {
                Name = "Ajman",
                Areas = new List<string> { Areas.Manama, Areas.AlRashidiya, Areas.AlNuaimiya },
                Country = UAE
            };
            var sharjah = new City
            {
                Name = "Sharjah",
                Areas = new List<string> { Areas.AlNahda, Areas.AlQasimia, Areas.AlKhan },
                Country = UAE
            };

            var cities = new List<City> { dubai, abuDhabi, ajman, sharjah };
            await service.CreateMany(cities.ToArray());
        }

        public async Task SeedLoyaltyTypes(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<ILoyaltyTypeRepository>();
            var res = await service.GetAll();
            if (res.Any())
                return;

            var loyaltyTypes = new List<LoyaltyType>
            {
                new LoyaltyType{ Name = "One Free House Beverage" },
                new LoyaltyType{ Name = "One Free Main Menu Item" },
                new LoyaltyType{ Name = "One Free Brunch" },
                new LoyaltyType{ Name = "One Free Starter Item" },
                new LoyaltyType{ Name = "One Free Bottle of House Wine" }
            };

            foreach (var loyaltyType in loyaltyTypes)
            {
                await service.Create(loyaltyType);
            }
        }

        public async Task SeedOfferTypes(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<IOfferTypeRepository>();
            var res = await service.GetAll();
            if (res.Any())
                return;

            var offerTypes = new List<OfferType>
            {
                new OfferType{ Name = "Buy 1 Get 1 Free (House Beverage)" },
                new OfferType{ Name = "Buy 2 Get 1 Free (House Beverage)" },
                new OfferType{ Name = "Buy 3 Get 1 Free (House Beverage)" },
                new OfferType{ Name = "Buy 4 Get 1 Free (House Beverage)" },
                new OfferType{ Name = "Buy 5 Get 1 Free (House Beverage)" },

                new OfferType{ Name = "Buy 1 Get 1 Free (Bottles of Wine or Spirits)" },
                new OfferType{ Name = "Buy 2 Get 1 Free (Bottles of Wine or Spirits)" },

                new OfferType{ Name = "Buy 1 Get 1 Free (Food Main Course)" },
                new OfferType{ Name = "Buy 2 Get 1 Free (Food Main Course)" },
                new OfferType{ Name = "Buy 3 Get 1 Free (Food Main Course)" },

                new OfferType{ Name = "Buy 1 Get 1 Free (Starters)" },
                new OfferType{ Name = "Buy 2 Get 1 Free (Starters)" },
                new OfferType{ Name = "Buy 3 Get 1 Free (Starters)" },

                new OfferType{ Name = "Buy 1 Get 1 Free (Brunch)" },
                new OfferType{ Name = "Buy 2 Get 1 Free (Brunch)" },
                new OfferType{ Name = "Buy 3 Get 1 Free (Brunch)" },
                new OfferType{ Name = "Buy 4 Get 1 Free (Brunch)" },

                new OfferType{ Name = "10% Discount off the bill" },
                new OfferType{ Name = "15% Discount off the bill" },
                new OfferType{ Name = "20% Discount off the bill" },
                new OfferType{ Name = "25% Discount off the bill" },
                new OfferType{ Name = "30% Discount off the bill" },
                new OfferType{ Name = "50% Discount off the bill" },

                new OfferType{ Name = "One Complimentary House Beverage from OutOut" },
            };

            foreach (var offerType in offerTypes)
            {
                await service.Create(offerType);
            }
        }

        public async Task SeedEvents(IServiceProvider serviceProvider)
        {
            var cityService = serviceProvider.GetRequiredService<ICityRepository>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var cities = await cityService.GetAll();
            var dubaiCity = cities.ElementAt(0);
            var abudhabiCity = cities.ElementAt(1);
            var ajmanCity = cities.ElementAt(2);

            var service = serviceProvider.GetRequiredService<IEventRepository>();
            var categroyService = serviceProvider.GetRequiredService<ICategoryRepository>();
            var superAdminId = (await userManager.FindByEmailAsync("super_admin@outout.com")).Id;
            var res = await service.GetAll();
            if (res.Any())
                return;

            var Packages = new List<List<EventPackage>>();
            for (var i = 0; i < 6; i++)
            {
                var newPackage = new List<EventPackage>
                        {
                            new EventPackage { Id = ObjectId.GenerateNewId().ToString(), Title = "Soft Drinks Package", Price = 299, TicketsNumber = 50, RemainingTickets = 50},
                            new EventPackage { Id = ObjectId.GenerateNewId().ToString(), Title = "House Package", Price = 399, TicketsNumber = 150, RemainingTickets = 150}
                        };
                Packages.Add(newPackage);
            }


            var event1 = new Event
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "Secret Garden Brunch",
                Image = "event.jpg",
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry.",
                Code = "4444",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(55.2796, 25.1988)),
                    City = new CitySummary { Id = dubaiCity.Id, Name = dubaiCity.Name },
                    Area = "DIFC"
                },
                Occurrences = new List<EventOccurrence>
                {
                    new EventOccurrence
                    {
                        StartDate = DateTime.UtcNow.Date,
                        EndDate = DateTime.UtcNow.Date.AddDays(3),
                        StartTime = new TimeSpan(15,0,0),
                        EndTime = new TimeSpan(18,0,0),
                        Packages = Packages[0]
                    }
                },
                IsFeatured = true,
                TicketsNumber = 2000,
                FacebookLink = "https://www.facebook.com/",
                YoutubeLink = "https://www.youtube.com/",
                PhoneNumber = "026780522",
                Email = "super_admin@outout.com",
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Party"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Outdoor"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Happy Hour"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Family"),
                }
            };

            var event2 = new Event
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "Happy Hour",
                Image = "event.jpg",
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry.",
                Code = "4444",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(54.3832, 24.4959)),
                    City = new CitySummary { Id = abudhabiCity.Id, Name = abudhabiCity.Name },
                    Area = "Masdar City"
                },
                Occurrences = new List<EventOccurrence>
                {
                    new EventOccurrence
                    {
                        StartDate = DateTime.UtcNow.Date,
                        EndDate = DateTime.UtcNow.Date.AddDays(2),
                        StartTime = new TimeSpan(19,0,0),
                        EndTime = new TimeSpan(19,0,0),
                        Packages = Packages[1]
                    }
                },
                IsFeatured = true,
                TicketsNumber = 2000,
                FacebookLink = "https://www.facebook.com/",
                YoutubeLink = "https://www.youtube.com/",
                PhoneNumber = "026780522",
                Email = "super_admin@outout.com",
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Party"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Concert"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Clubbing"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Family"),
                }
            };

            var event3 = new Event
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "Lighthouse Party",
                Image = "event.jpg",
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry.",
                Code = "4444",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(55.5136, 25.4052)),
                    City = new CitySummary { Id = ajmanCity.Id, Name = ajmanCity.Name },
                    Area = "Ajmann Marina"
                },
                Occurrences = new List<EventOccurrence>
                {
                    new EventOccurrence
                    {
                        StartDate = DateTime.UtcNow.Date,
                        EndDate = DateTime.UtcNow.Date.AddDays(5),
                        StartTime = new TimeSpan(16,0,0),
                        EndTime = new TimeSpan(19,0,0),
                        Packages = Packages[2]
                    }
                },
                TicketsNumber = 2000,
                PhoneNumber = "026780522",
                FacebookLink = "https://www.facebook.com/",
                YoutubeLink = "https://www.youtube.com/",
                Email = "super_admin@outout.com",
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Brunch"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Clubbing"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Family"),
                }
            };

            var event4 = new Event
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "The Easy Lunch",
                Image = "event.jpg",
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry.",
                Code = "4444",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(55.5136, 25.4052)),
                    City = new CitySummary { Id = ajmanCity.Id, Name = ajmanCity.Name },
                    Area = "Ajmann Marina"
                },
                Occurrences = new List<EventOccurrence>
                {
                    new EventOccurrence
                    {
                        StartDate = DateTime.UtcNow.Date,
                        EndDate = DateTime.UtcNow.Date.AddDays(5),
                        StartTime = new TimeSpan(16,0,0),
                        EndTime = new TimeSpan(19,0,0),
                        Packages = Packages[3]
                    }
                },
                PhoneNumber = "026780522",
                FacebookLink = "https://www.facebook.com/",
                YoutubeLink = "https://www.youtube.com/",
                Email = "super_admin@outout.com",
                TicketsNumber = 2000,
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Latino"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Clubbing"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Family"),
                }
            };

            var event5 = new Event
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "Wing it",
                Image = "event.jpg",
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry.",
                Code = "4444",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(55.5136, 25.4052)),
                    City = new CitySummary { Id = ajmanCity.Id, Name = ajmanCity.Name },
                    Area = "Ajmann Marina"
                },
                Occurrences = new List<EventOccurrence>
                {
                    new EventOccurrence
                    {
                        StartDate = DateTime.UtcNow.Date,
                        EndDate = DateTime.UtcNow.Date.AddDays(5),
                        StartTime = new TimeSpan(16,0,0),
                        EndTime = new TimeSpan(19,0,0),
                        Packages = Packages[4]
                    }
                },
                PhoneNumber = "026780522",
                TicketsNumber = 2000,
                FacebookLink = "https://www.facebook.com/",
                YoutubeLink = "https://www.youtube.com/",
                Email = "super_admin@outout.com",
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Latino"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Clubbing"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Family"),
                }
            };

            var event6 = new Event
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "Alba Eats",
                Image = "event.jpg",
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry.",
                Code = "4444",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(55.5136, 25.4052)),
                    City = new CitySummary { Id = ajmanCity.Id, Name = ajmanCity.Name },
                    Area = "Ajmann Marina"
                },
                Occurrences = new List<EventOccurrence>
                {
                    new EventOccurrence
                    {
                        StartDate = DateTime.UtcNow.Date,
                        EndDate = DateTime.UtcNow.Date.AddDays(5),
                        StartTime = new TimeSpan(16,0,0),
                        EndTime = new TimeSpan(19,0,0),
                        Packages = Packages[5]
                    }
                },
                PhoneNumber = "026780522",
                FacebookLink = "https://www.facebook.com/",
                YoutubeLink = "https://www.youtube.com/",
                Email = "super_admin@outout.com",
                TicketsNumber = 2000,
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Latino"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Clubbing"),
                    await categroyService.GetByTypeAndName(TypeFor.Event, "Family"),
                }
            };

            await service.CreateEventWithOccurrences(event1);
            await service.CreateEventWithOccurrences(event2);
            await service.CreateEventWithOccurrences(event3);
            await service.CreateEventWithOccurrences(event4);
            await service.CreateEventWithOccurrences(event5);
            await service.CreateEventWithOccurrences(event6);
        }
        public async Task AddVenuesToEvents(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<IVenueRepository>();
            var eventService = serviceProvider.GetRequiredService<IEventRepository>();

            var events = await eventService.GetAll();
            var venues = await service.GetAll();

            var venueSummary1 = new VenueSummary
            {
                Id = venues.FirstOrDefault().Id,
                Name = venues.FirstOrDefault().Name,
                OpenTimes = venues.FirstOrDefault().OpenTimes,
                Logo = venues.FirstOrDefault().Logo,
                Location = venues.FirstOrDefault().Location
            };
            events.FirstOrDefault().Venue = venueSummary1;

            var venueSummary2 = new VenueSummary
            {
                Id = venues.ElementAt(1).Id,
                Name = venues.ElementAt(1).Name,
                OpenTimes = venues.ElementAt(1).OpenTimes,
                Logo = venues.ElementAt(1).Logo,
                Location = venues.ElementAt(1).Location
            };
            events.ElementAt(1).Venue = venueSummary2;

            var venueSummary3 = new VenueSummary
            {
                Id = venues.ElementAt(2).Id,
                Name = venues.ElementAt(2).Name,
                OpenTimes = venues.ElementAt(2).OpenTimes,
                Logo = venues.ElementAt(2).Logo,
                Location = venues.ElementAt(2).Location
            };
            events.ElementAt(2).Venue = venueSummary3;

            events.ForEach(a => eventService.Update(a));
        }
        public async Task SeedVenues(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<IVenueRepository>();
            var eventService = serviceProvider.GetRequiredService<IEventRepository>();
            var categroyService = serviceProvider.GetRequiredService<ICategoryRepository>();
            var tcService = serviceProvider.GetRequiredService<ITermsAndConditionsRepository>();
            var cityService = serviceProvider.GetRequiredService<ICityRepository>();
            var loyaltyTypeService = serviceProvider.GetRequiredService<ILoyaltyTypeRepository>();
            var offerTypeService = serviceProvider.GetRequiredService<IOfferTypeRepository>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var loyaltyTypes = await loyaltyTypeService.GetAll();
            var offerTypes = await offerTypeService.GetAll();
            var superAdminId = (await userManager.FindByEmailAsync("super_admin@outout.com")).Id;

            var res = await service.GetAll();
            if (res.Any())
                return;

            var eventsIds = (await eventService.GetAll()).Select(a => a.Id).ToList();

            var cities = await cityService.GetAll();
            var dubaiCity = cities.ElementAt(0);
            var abudhabiCity = cities.ElementAt(1);
            var ajmanCity = cities.ElementAt(2);

            var venueDubaiMall = new Venue
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "The Dubai Mall",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(55.2796, 25.1988)),
                    City = new CitySummary { Id = dubaiCity.Id, Name = dubaiCity.Name },
                    Area = "DIFC",
                    Description = "1 Sheikh Mohammed bin Rashid Blvd - Downtown Dubai - Dubai - United Arab Emirates"
                },
                PhoneNumber = "+97148888888",
                OpenTimes = new List<AvailableTime>{ new AvailableTime
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday },
                    From = new TimeSpan(09, 0, 0),
                    To = new TimeSpan(19, 0, 0)
                } },
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.",
                Gallery = new List<string>() { "gallery1.jpg", "gallery2.jpg" },
                OffersCode = "0000",
                LoyaltyCode = "4444",
                Loyalty = new Loyalty
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    IsActive = true,
                    Stars = LoyaltyStars.TenStars,
                    MaxUsage = 0,
                    Type = loyaltyTypes.ElementAt(1),
                    ValidOn = new List<AvailableTime>{new AvailableTime
                    {
                        Days = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                        From = new TimeSpan(6, 0, 0),
                        To = new TimeSpan(19, 0, 0)
                    } },
                },
                //Offers = new List<Offer>
                //{
                //    CreateActiveNonExpiredOffer(OfferTypes.BuyOneGetOne_HouseBeverage),
                //    CreateActiveNonExpiredOffer(OfferTypes.BillDiscount10Percent),
                //    CreateActiveNonExpiredOffer(OfferTypes.BuyFourGetOne_Brunch),
                //},
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Nightclub"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Cigar Lounge"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Cocktail Bar"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Italian"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Tapas"),
                },
                Menu = "71738897-31e2-4454-8eae-3914dd8218be.pdf",
                SelectedTermsAndConditions = tcService.GetAll().Result.Select(a => a.Id).Take(10).ToList(),
                Logo = "logo.jpg",
                Background = "logo.jpg",
                Events = eventsIds
            };
            var venueDubaiRitz = new Venue
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "Ritz-Carlton",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(55.2794, 25.2125)),
                    City = new CitySummary { Id = dubaiCity.Id, Name = dubaiCity.Name },
                    Area = "DIFC",
                    Description = "1 Sheikh Mohammed bin Rashid Blvd - Downtown Dubai - Dubai - United Arab Emirates"
                },
                OpenTimes = new List<AvailableTime> { new AvailableTime
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday },
                    From = new TimeSpan(09, 0, 0),
                    To = new TimeSpan(19, 0, 0)
                } },
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "German"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "American"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "European"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Russian"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Japanese"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Pub"),
                },
                Menu = "71738897-31e2-4454-8eae-3914dd8218be.pdf",
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.",
                Logo = "logo.jpg",
                Background = "logo.jpg",
                OffersCode = "0000",
                Offers = new List<Offer>
                {
                    CreateActiveExpiredOffer(offerTypes.ElementAt(1)),
                    CreateActiveNonExpiredOffer(offerTypes.ElementAt(2)),
                    CreateActiveNonExpiredOffer(offerTypes.ElementAt(3)),
                    CreateActiveNonExpiredOffer(offerTypes.ElementAt(4)),
                    CreateDeactivedNonExpiredOffer(offerTypes.ElementAt(5)),
                },
                LoyaltyCode = "4444",
                Loyalty = new Loyalty
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    IsActive = true,
                    Stars = LoyaltyStars.TenStars,
                    MaxUsage = 0,
                    Type = loyaltyTypes.ElementAt(2),
                    ValidOn = new List<AvailableTime>{new AvailableTime
                    {
                        Days = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                        From = new TimeSpan(6, 0, 0),
                        To = new TimeSpan(19, 0, 0)
                    } },
                },
                Events = new List<string> { eventsIds.ElementAt(1) }
            };
            var venueAbuDhabi = new Venue
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "Abu Dhabi Mall",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(54.3832, 24.4959)),
                    City = new CitySummary { Id = abudhabiCity.Id, Name = abudhabiCity.Name },
                    Area = "Al Zahiyah",
                    Description = "1 Sheikh Mohammed bin Rashid Blvd - Downtown Dubai - Dubai - United Arab Emirates"
                },
                PhoneNumber = "+97148888888",
                OpenTimes = new List<AvailableTime> { new AvailableTime
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Monday },
                    From = new TimeSpan(0, 0, 0),
                    To = new TimeSpan(19, 0, 0)
                } },
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "South American"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "African"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Buffet"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Bar"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Japanese"),
                },
                Gallery = new List<string>() { "gallery1.jpg", "gallery2.jpg" },
                Menu = "71738897-31e2-4454-8eae-3914dd8218be.pdf",
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.",
                SelectedTermsAndConditions = tcService.GetAll().Result.Select(a => a.Id).Take(8).ToList(),
                OffersCode = "0000",
                LoyaltyCode = "4444",
                Loyalty = new Loyalty
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    IsActive = true,
                    Stars = LoyaltyStars.TenStars,
                    MaxUsage = 0,
                    Type = loyaltyTypes.ElementAt(2),
                    ValidOn = new List<AvailableTime>{new AvailableTime
                    {
                        Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                        From = new TimeSpan(6, 0, 0),
                        To = new TimeSpan(19, 0, 0)
                    } },
                },
                Logo = "logo.jpg",
                Background = "logo.jpg",
                Events = new List<string> { eventsIds.FirstOrDefault() }
            };
            var venueAlexandria = new Venue
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "Alexandria",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(29.9187, 31.2001)),
                    City = new CitySummary { Id = abudhabiCity.Id, Name = abudhabiCity.Name },
                    Area = "Sidi Gaber",
                    Description = "1 Sheikh Mohammed bin Rashid Blvd - Downtown Dubai - Dubai - United Arab Emirates"
                },
                PhoneNumber = "+97148888888",
                OpenTimes = new List<AvailableTime> {  new AvailableTime
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday },
                    From = new TimeSpan(0, 0, 0),
                    To = new TimeSpan(19, 0, 0)
                } },
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Greek"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Arabic"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Asian"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Cigar Lounge"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Japanese"),
                },
                Gallery = new List<string>() { "gallery1.jpg", "gallery2.jpg" },
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.",
                SelectedTermsAndConditions = tcService.GetAll().Result.Select(a => a.Id).Take(15).ToList(),
                OffersCode = "0000",
                LoyaltyCode = "4444",
                Menu = "71738897-31e2-4454-8eae-3914dd8218be.pdf",
                Loyalty = new Loyalty
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    IsActive = true,
                    Stars = LoyaltyStars.TenStars,
                    MaxUsage = 0,
                    Type = loyaltyTypes.ElementAt(3),
                    ValidOn = new List<AvailableTime>{new AvailableTime
                    {
                        Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                        From = new TimeSpan(6, 0, 0),
                        To = new TimeSpan(19, 0, 0)
                    } },
                },
                //Offers = new List<Offer>
                //{
                //    CreateActiveNonExpiredOffer(OfferTypes.BuyTwoGetOne_FoodMainCourse),
                //    CreateActiveNonExpiredOffer(OfferTypes.BuyTwoGetOne_FoodMainCourse)
                //},
                Logo = "logo.jpg",
                Background = "logo.jpg",
                Events = new List<string> { eventsIds.ElementAt(2) }
            };
            var venueAtlantis = new Venue
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Name = "Atlantis The Palm",
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(29.9187, 31.2001)),
                    City = new CitySummary { Id = dubaiCity.Id, Name = dubaiCity.Name },
                    Area = "The Palm",
                    Description = "1 Sheikh Mohammed bin Rashid Blvd - Downtown Dubai - Dubai - United Arab Emirates"
                },
                OpenTimes = new List<AvailableTime> {  new AvailableTime
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                    From = new TimeSpan(07, 0, 0),
                    To = new TimeSpan(19, 0, 0)
                } },
                Categories = new List<Category>
                {
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Pub"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Bar"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Lounge"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Outdoor"),
                    await categroyService.GetByTypeAndName(TypeFor.Venue, "Beach Club"),
                },
                Menu = "71738897-31e2-4454-8eae-3914dd8218be.pdf",
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.",
                OffersCode = "0000",
                LoyaltyCode = "4444",
                Loyalty = new Loyalty
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    IsActive = true,
                    Stars = LoyaltyStars.TenStars,
                    MaxUsage = 0,
                    Type = loyaltyTypes.ElementAt(1),
                    ValidOn = new List<AvailableTime>{new AvailableTime
                    {
                        Days = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                        From = new TimeSpan(6, 0, 0),
                        To = new TimeSpan(19, 0, 0)
                    } },
                },
                Logo = "logo.jpg",
                Background = "logo.jpg",
                Events = eventsIds
            };
            await service.Create(venueAbuDhabi);
            await service.Create(venueDubaiRitz);
            await service.Create(venueAlexandria);
            await service.Create(venueDubaiMall);
            await service.Create(venueAtlantis);
        }

        private static Offer CreateActiveExpiredOffer(OfferType offerType)
        {
            return new Offer
            {
                Type = offerType,
                ValidOn = new List<AvailableTime> {new AvailableTime
                {
                    Days = new List<DayOfWeek> { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday },
                    From = new TimeSpan(0, 0, 0),
                    To = new TimeSpan(19, 59, 59)
                }},
                ExpiryDate = new DateTime(2020, 1, 1),
                IsActive = true,
                MaxUsagePerYear = OfferUsagePerYear.FiveTimes
            };
        }
        private static Offer CreateActiveNonExpiredOffer(OfferType offerType)
        {
            return new Offer
            {
                Type = offerType,
                ValidOn = new List<AvailableTime> {new AvailableTime
                {
                    Days = new List<DayOfWeek> { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday },
                    From = new TimeSpan(0, 0, 0),
                    To = new TimeSpan(19, 59, 59)
                } },
                ExpiryDate = new DateTime(2022, 1, 1),
                IsActive = true,
                MaxUsagePerYear = OfferUsagePerYear.FiveTimes
            };
        }
        private static Offer CreateDeactivedNonExpiredOffer(OfferType offerType)
        {
            return new Offer
            {
                Type = offerType,
                ValidOn = new List<AvailableTime> {new AvailableTime
                {
                    Days = new List<DayOfWeek> { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday },
                    From = new TimeSpan(0, 0, 0),
                    To = new TimeSpan(19, 59, 59)
                } },
                ExpiryDate = new DateTime(2022, 1, 1),
                IsActive = false,
                MaxUsagePerYear = OfferUsagePerYear.FiveTimes
            };
        }

        public async Task SeedFAQ(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<IFAQRepository>();
            var res = await service.GetAll();
            if (res.Any())
                return;

            var faq = new List<FAQ>
            {
                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "WHAT CURRENCY IS OUTOUT?",
                    Answer = "At the moment OutOut is in Arab Emirate Dirham (AED)"
                },

                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "HOW IS OUTOUT FOLLOWING COVID SAFTEY STANDARDS?",
                    Answer = "With the return of live events back to the UAE scene, our team approaches the current situation very cautiously and takes all the steps to protect its staff and event attendees. Our Cashiers, Scanners, and Supervisors, as well as the rest of the team, are following strict measures, we also ask the the venues we work with also follow strict instructions and the guidelines which have been put in place by the United Arab Emirates Health Authorities. Here are some of the measures taken below:\n\n" +
                        "• Being educated with circulating up - to - date Covid - 19 DMHS's updates and guidelines.\n" +
                        "• Prioritizing online meetings / briefings at all times.\n" +
                        "• Maximizing implementation of contactless methods for wrist banding, ticket scanning & purchasing, and other operational activities(with online ticket purchase being encouraged as a payment solution)\n" +
                        "• Being temperature - checked 1 day prior to the event they work at, 5 hours before, and during the event. \n" +
                        "• Following social distance rules by keeping 2 meters apart from all event’s attendees everywhere including washrooms, queuing spaces, common areas.\n\n" +

                        "Wearing protective equipment at all times: \n" +
                        "• WHO recommended face-covering masks - changing them every half an hour\n" +
                        "• Protective gloves - changing them every half an hour\n" +
                        "• Face shields - to ultimately prohibit any possible spread of droplets\n\n" +

                        "• Being instructed not to touch their eyes, nose, or mouth.\n" +
                        "• Sanitizing and disinfecting their work areas (commonly touched surfaces like tables, doorknobs, light switches, countertops, handles) with sanitizers.\n" +
                        "• Washing hands every hour for at least 20 seconds as per WHO guidance.\n" +
                        "• Being examined daily not to have any COVID-19 symptoms such as fever or cough.Policed not to be attending the event if they have had any contact with a person diagnosed with or suspected to be unwell in the past 2 weeks.\n"
                },

                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "CAN I CHANGE THE LANGUAGE OF THE APP?",
                    Answer = "At the moment OutOut is only supporting English, however we are looking at Arabic at a later release."
                },

                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "HOW CAN I SEE MY BOOKINGS OR TICKETS?",
                    Answer = "If you open your app and click on the profile button on the bottom, you’ll then see your account information. Click on My Bookings and here you’ll see all your bookings and reservations and tickets. "
                },

                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "CAN I EXCHANGE OR REFUND MY TICKETS?",
                    Answer = "Each venue has its own restrictions regarding cancellations and refunds. Some venues run a no cancellations or refunds policy, other may charge a fee and others allow changes depending on circumstances. Please refer to the Terms and Conditions on the bottom of the page of the event you are purchasing tickets for."
                },

                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "I BOUGHT MULTIPLE TICKETS FOR FRIENDS AND FAMILY, HOW CAN I SHARE THEM?",
                    Answer = "You’re able to click on your tickets and click the share button on the top right.This will then allow you to share the ticket via whats app, email, pdf."
                },

                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "IF I SHARE MY TICKET, CAN SOMEONE USE IT MULTIPLE TIMES?",
                    Answer = "If your ticket has already been used before, it will scan red which means it has already been used. Please only share tickets with the persons you are attending with and trust. OutOut and the venues accept no responsibility for tickets which may have been compromised or used previously."
                },

                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "HOW DO I USE MY TICKET?",
                    Answer = "You can show your Ticket on entry to the venue and the ticketing team will scan your ticket or enter a code. If the ticket is legitimate and has not been used, it will scan Green and you will allowed entrance. If the ticket is fake or has been used before it will scan Red.\n" +
                         "Tickets are found in Your Bookings section of the application."
                },

                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "MY LOYALTY HAS BEEN RESET?",
                    Answer = "Sometimes venues may change or alter their loyalty offerings or start a new loyalty promotion. Your loyalty for that offer may reset. However all your other loyalty stars will remain."
                },

               new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "CAN WE USE MORE THAN 1 OFFER PER DAY?",
                    Answer = "Each user is allowed to use 1 offer in a 24 hour period in each venue. However there are no limits on how many venues a user can redeem the offers in the same period.\n" +
                         "If you and your friends would like to avail more than 1 offer in the venue you can each download the app and use one offer per account.Venues may limit this to a maximum of 4 offers per group per sitting."
                },

                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "MY CREDIT CARD IS NOT WORKING?",
                    Answer = "Some credit cards which are based from banks outside the UAE may not work. Please try to use a valid UAE Card for purchases. \n" +
                         "For any assistance please contact us at payments@outout.com"
                },

                new FAQ
                {
                    QuestionNumber = await service.GenerateLastIncrementalNumber("FAQ Number"),
                    Question = "I NEED HELP, IS THERE ANYONE I CAN CONTACT?",
                     Answer = "For any enquiries please email us at help@outout.com and a team member will contact you within 24-48 working hours. "
                }
            };

            foreach (var question in faq)
            {
                await service.Create(question);
            }
        }
        public async Task SeedTermsAndConditions(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<ITermsAndConditionsRepository>();
            var res = await service.GetAll();

            if (res.Any())
                return;

            var TermsAndConditions = new List<string>()
            {
                "Venue has a strict over 21 policy.",
                "Venue accepts Children.",
                "Venue has a dress code policy.",
                "Venue accepts pre booked guests only.",
                "Venue has the right to refuse entry.",
                "All guests must present an Original Emirates ID, International Passport or UAE Driving Licence upon entry into the venue.",
                "All guests must follow the local laws and rules.",
                "All guests must follow Covid-19 rules and regulations.",
                "Event Tickets and Bookings can not be exchanged for cash or any other purpose other than it’s intended use.",
                "Event Tickets and Bookings are non refundable unless stated otherwise.",
                "Event Tickets and Bookings can not be duplicated.",
                "All Event Tickets and Bookings sales are final.",
                "Loyalty Stars can only be claimed one per 24 hours in each venue.",
                "Once Loyalty is redeemed, the loyalty will be reset for the venue.",
                "Management of the venue are the only persons who are allowed to redeem Loyalty.",
                "Abuse of the Loyalty feature may result in cancelation of your OutOut account.",
                "A maximum of 1 Offers may be used for a group of 1 - 3 people sitting on the same table.",
                "A maximum of 2 Offers may be used for a group of 4 - 5 people sitting on the same table.",
                "A maximum of 3 Offers may be used for a group of 6 or more people sitting on the same table.",
                "Management have the right to refuse Offers.",
                "Offers can not be used in conjunction with any other offer.",
                "Offers can only be used one in each venue per sitting.",
                "Offers are not applicable to happy hour and other discounted products.",
                "Offers are not available on public holidays or special days(Eid, Christmas Eve, Christmas Day, New Years Eve, National Day or Special Events held by the venue on that day).",
            };

            foreach (var tc in TermsAndConditions)
            {
                await service.Create(new TermsAndConditions { TermCondition = tc, IsActive = true });
            }
        }
        public async Task SeedCategories(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<ICategoryRepository>();
            var res = await service.GetAll();

            if (res.Any())
                return;

            var venueCategories = new List<string>()
            {
                "Pub",
                "Bar",
                "Lounge",
                "Outdoor",
                "Beach Club",
                "Nightclub",
                "Cigar Lounge",
                "Cocktail Bar",
                "Tapas",
                "Italian",
                "Greek",
                "Arabic",
                "Asian",
                "Japanese",
                "Chinese",
                "German",
                "American",
                "European",
                "Russian",
                "International",
                "South American",
                "African",
                "Buffet",
                "Garden",
                "Park",
                "Lebanese",
                "Turkish",
                "Jamaican",
                "French",
                "Bistro",
                "Pool Bar",
                "Coffee Shop",
                "Art Gallery",
                "Arena",
                "Museum",
                "Sports Club",
                "Stadium",
                "Ballroom",
                "Irish Bar",
                "Food Truck",
                "Fine Dining",
                "Fast Food",
                "Shopping Mall",
                "Pop Up",
            };

            foreach (var categoryName in venueCategories)
            {
                await service.Create(new Category { Name = categoryName, TypeFor = TypeFor.Venue, IsActive = true, Icon = categoryName + ".png" });
            }

            var eventCategories = new List<string>()
            {
                "Brunch",
                "Ladies Night",
                "Nightlife",
                "Clubbing",
                "Party",
                "Concert",
                "Food",
                "Live Music",
                "DJ",
                "Sports",
                "Family",
                "Outdoor",
                "Lifestyle",
                "Comedy",
                "Wellness",
                "Theatre",
                "Valentines Day",
                "Jazz",
                "Market",
                "Urban",
                "Other",
                "Opera",
                "Happy Hour",
                "Festive, Desi",
                "Exhibition",
                "Latino",
                "Boat Party",
                "Pool Party",
                "Dinner Party",
                "Quiz Night",
                "Karaoke",
                "Fashion",
                "Oktoberfest",
                "Fitness",
                "Shopping",
                "Grand Opening",
                "Arabic",
                "Ramadan",
                "New Years Eve",
                "Christmas",
                "St Patricks Day",
                "Date Night",
            };
            foreach (var categoryName in eventCategories)
            {
                await service.Create(new Category { Name = categoryName, TypeFor = TypeFor.Event, IsActive = true, Icon = categoryName + ".png" });
            }
        }
    }
}
