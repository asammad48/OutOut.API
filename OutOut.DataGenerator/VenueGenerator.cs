using MongoDB.Bson;
using OutOut.Constants.Enums;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;
using MongoDB.Driver.GeoJsonObjectModel;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace OutOut.DataGenerator
{
    public class VenueGenerator
    {
        private readonly IVenueRepository _venueRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITermsAndConditionsRepository _termsAndConditionsRepository;
        private readonly ICityRepository _cityRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public VenueGenerator(IVenueRepository venueRepository, ICategoryRepository categoryRepository, ITermsAndConditionsRepository termsAndConditionsRepository, ICityRepository cityRepository, UserManager<ApplicationUser> userManager)
        {
            _venueRepository = venueRepository;
            _categoryRepository = categoryRepository;
            _termsAndConditionsRepository = termsAndConditionsRepository;
            _cityRepository = cityRepository;
            _userManager = userManager;
        }

        public async Task Generate()
        {
            var venueNames = new List<string>() { "Burj Khalifa", "Ski Dubai", "Dubai Greek Tower", "Mall Of Emirates", "Palm Islands", "Dubai Marina", "Abu Dhabi National Exhibition Centre", "Dubai Creek", "The Dubai Fountain", "Palm Jumeirah", "Burj Al Arab" };
            foreach (var name in venueNames)
            {
                var venue = await GenerateVenue();
                venue.Name = name;
                await _venueRepository.Create(venue);
            }
        }

        private async Task<Venue> GenerateVenue()
        {
            var dubaiCity = (await _cityRepository.GetAll()).FirstOrDefault();
            var superAdminId = (await _userManager.FindByEmailAsync("super_admin@outout.com")).Id;

            return new Venue
            {
                CreationDate = DateTime.UtcNow,
                CreatedBy = superAdminId,
                Id = ObjectId.GenerateNewId().ToString(),
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(55.2796, 25.1988)),
                    City = new CitySummary { Id = dubaiCity.Id, Name = dubaiCity.Name },
                    Area = "DIFC",
                    Description = "1 Sheikh Mohammed bin Rashid Blvd - Downtown Dubai - Dubai - United Arab Emirates"
                },
                PhoneNumber = "+97148888888",
                Categories = new List<Category>
                {
                    await _categoryRepository.GetByTypeAndName(TypeFor.Venue, "Nightclub"),
                    await _categoryRepository.GetByTypeAndName(TypeFor.Venue, "Cigar Lounge"),
                    await _categoryRepository.GetByTypeAndName(TypeFor.Venue, "Cocktail Bar"),
                    await _categoryRepository.GetByTypeAndName(TypeFor.Venue, "Italian"),
                },
                Menu = "71738897-31e2-4454-8eae-3914dd8218be.pdf",
                SelectedTermsAndConditions = _termsAndConditionsRepository.GetAll().Result.Select(a => a.Id).Take(10).ToList(),
                Logo = "logo.jpg",
                Background = "logo.jpg",
                OpenTimes = new List<AvailableTime> { GenerateAvailableTime() },
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.",
                Gallery = new List<string>() { "gallery1.jpg", "gallery2.jpg" },
                OffersCode = "0000",
                LoyaltyCode = "4444",
            };
        }

        private static T Generate<T>(List<T> list)
        {
            Random rnd = new Random();
            int r = rnd.Next(list.Count);
            return list[r];
        }

        private static AvailableTime GenerateAvailableTime()
        {
            var listOfDays = new List<DayOfWeek>()
            {
                DayOfWeek.Saturday, DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Tuesday, DayOfWeek.Thursday
            };

            var listOfAvailableTimes = new List<AvailableTime>();

            foreach (var item in listOfDays)
            {
                listOfAvailableTimes.Add(new AvailableTime
                {
                    Days = new List<DayOfWeek> { item },
                    From = new TimeSpan(0, 0, 0),
                    To = new TimeSpan(23, 59, 59),
                });
            }

            return Generate(listOfAvailableTimes);
        }
    }
}
