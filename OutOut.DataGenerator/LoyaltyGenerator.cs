using MongoDB.Bson;
using OutOut.Constants.Enums;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;

namespace OutOut.DataGenerator
{
    public class LoyaltyGenerator
    {
        private readonly IVenueRepository _venueRepository;
        private readonly ILoyaltyTypeRepository _loyaltyTypeRepository;

        public LoyaltyGenerator(IVenueRepository venueRepository, ILoyaltyTypeRepository loyaltyTypeRepository)
        {
            _venueRepository = venueRepository;
            _loyaltyTypeRepository = loyaltyTypeRepository;
        }

        public async Task RemoveAllLoyalty()
        {
            var allVenues = await _venueRepository.GetAll();
            foreach (var venue in allVenues)
            {
                venue.Loyalty = new Loyalty();
                await _venueRepository.Update(venue);
            }
        }

        public async Task Generate()
        {
            await RemoveAllLoyalty();

            var allVenues = await _venueRepository.GetAll();

            //allVenues.FirstOrDefault().Loyalty = GenerateDimmedLoyalty();
            //await _venueRepository.Update(allVenues.FirstOrDefault());

            foreach (var venue in allVenues)
            {
                venue.Loyalty = await GenerateLoyalty();
                await _venueRepository.Update(venue);
            }
        }

        private async Task<Loyalty> GenerateLoyalty()
        {
            var loyaltyTypes = await _loyaltyTypeRepository.GetAll();
            var stars = new List<int> { 5, 10 };
            return new Loyalty
            {
                Id = ObjectId.GenerateNewId().ToString(),
                IsActive = new Random().Next(2) == 1,
                MaxUsage = MaxUsage.Unlimited,
                ValidOn = new List<AvailableTime> { GenerateAvailableTime() },
                Type = loyaltyTypes[new Random().Next(loyaltyTypes.Count)],
                Stars = (LoyaltyStars)stars[new Random().Next(stars.Count)],
                AssignDate = DateTime.UtcNow,
            };
        }
        private Loyalty GenerateDimmedLoyalty()
        {
            return new Loyalty
            {
                Id = ObjectId.GenerateNewId().ToString(),
                IsActive = true,
                MaxUsage = MaxUsage.Unlimited,
                ValidOn = new List<AvailableTime>{new AvailableTime
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Friday },
                    From = new TimeSpan(23, 0, 0),
                    To = new TimeSpan(23, 0, 1)
                } },
                //Type = (LoyaltyTypes)new Random().Next(3),
                Stars = LoyaltyStars.TenStars,
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
                DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Tuesday, DayOfWeek.Thursday
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
