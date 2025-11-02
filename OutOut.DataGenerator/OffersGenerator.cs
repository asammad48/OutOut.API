using MongoDB.Bson;
using OutOut.Constants.Enums;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;

namespace OutOut.DataGenerator
{
    public class OffersGenerator
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IOfferTypeRepository _offerTypeRepository;

        public OffersGenerator(IVenueRepository venueRepository, IOfferTypeRepository offerTypeRepository)
        {
            _venueRepository = venueRepository;
            _offerTypeRepository = offerTypeRepository;
        }

        public async Task RemoveAllOffers()
        {
            var allVenues = await _venueRepository.GetAll();
            foreach (var venue in allVenues)
            {
                venue.Offers = new List<Offer>();
                await _venueRepository.Update(venue);
            }
        }

        public async Task Generate()
        {
            await RemoveAllOffers();

            var allVenues = await _venueRepository.GetAll();
            foreach (var venue in allVenues)
            {
                for (int i = 0; i < 20; i++)
                {
                    venue.Offers.Add(await GenerateOffer());
                }
                await _venueRepository.Update(venue);
            }
        }

        private async Task<Offer> GenerateOffer()
        {
            var offerTypes = await _offerTypeRepository.GetAll();
            var maxUsagePerYear = new List<int> { 3, 5 };
            return new Offer
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Image = "gallery1.jpg",
                IsActive = true,
                MaxUsagePerYear = (OfferUsagePerYear)maxUsagePerYear[new Random().Next(maxUsagePerYear.Count)],
                ValidOn = new List<AvailableTime> { GenerateAvailableTime() },
                Type = offerTypes[new Random().Next(offerTypes.Count)],
                ExpiryDate = GenerateExpiryDate(),
                AssignDate = DateTime.UtcNow
            };
        }

        private static T Generate<T>(List<T> list)
        {
            Random rnd = new Random();
            int r = rnd.Next(list.Count);
            return list[r];
        }

        private static DateTime GenerateExpiryDate()
        {
            var listOfValidDates = new List<DateTime>();
            for (int i = 1; i < 30; i++)
            {
                listOfValidDates.Add(new DateTime(2021, 12, i));
            }
            return Generate(listOfValidDates);
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
