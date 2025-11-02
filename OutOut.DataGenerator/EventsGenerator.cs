using MongoDB.Bson;
using OutOut.Constants.Enums;
using OutOut.Models.Models;
using OutOut.Persistence.Interfaces;
using MongoDB.Driver.GeoJsonObjectModel;
using OutOut.Models.Models.Embedded;
using OutOut.Models.Utils;

namespace OutOut.DataGenerator
{
    public class EventsGenerator
    {
        private readonly IEventRepository _eventRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICityRepository _cityRepository;

        public EventsGenerator(IEventRepository eventRepository, ICategoryRepository categoryRepository, ICityRepository cityRepository, IVenueRepository venueRepository)
        {
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
            _cityRepository = cityRepository;
            _venueRepository = venueRepository;
        }

        public async Task Generate()
        {
            var venues = await _venueRepository.GetAll();
            var eventNames = new List<string>() { "Secret Garden", "Garden Brunch", "The Happy Hour", "The Happy Hour Event", "The LightHouse Event", "Event Test", "LightHouse Event", "Brunch Event", "Happy Hour 2" };
            var counter = 0;
            foreach (var name in eventNames)
            {
                var eventObj = await GenerateEvent();
                eventObj.Name = name;

                var venueSummary = new VenueSummary
                {
                    Id = venues.ElementAt(counter).Id,
                    Name = venues.ElementAt(counter).Name,
                    OpenTimes = venues.ElementAt(counter).OpenTimes,
                    Logo = venues.ElementAt(counter).Logo
                };
                eventObj.Venue = venueSummary;
                await _eventRepository.CreateEventWithOccurrences(eventObj);

                venues.ElementAt(counter).Events.Clear();
                venues.ElementAt(counter).Events.Add(eventObj.Id);
                await _venueRepository.Update(venues.ElementAt(counter));

                counter++;
            }
        }

        private async Task<Event> GenerateEvent()
        {
            var dubaiCity = (await _cityRepository.GetAll()).FirstOrDefault();
            return new Event
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Location = new Location
                {
                    GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(55.2796, 25.1988)),
                    City = new CitySummary { Id = dubaiCity.Id, Name = dubaiCity.Name },
                    Area = "DIFC",
                    Description = "1 Sheikh Mohammed bin Rashid Blvd - Downtown Dubai - Dubai - United Arab Emirates"
                },
                PhoneNumber = "026780522",
                Categories = new List<Category>
                {
                    await _categoryRepository.GetByTypeAndName(TypeFor.Event, "Christmas"),
                    await _categoryRepository.GetByTypeAndName(TypeFor.Event, "New Years Eve"),
                    await _categoryRepository.GetByTypeAndName(TypeFor.Event, "Date Night"),
                    await _categoryRepository.GetByTypeAndName(TypeFor.Event, "Party"),
                },
                Email = "super_admin@outout.com",
                FacebookLink = "https://facebook.com",
                IsFeatured = new Random().Next(2) == 1,
                //TicketsNumber = 300,

                Occurrences = new List<EventOccurrence>
                {
                    new EventOccurrence
                    {
                        StartDate = UAEDateTime.Now.Date,
                        EndDate = UAEDateTime.Now.Date.AddDays(3),
                        StartTime = new TimeSpan(20,0,0),
                        EndTime = new TimeSpan(23,0,0),
                        Packages = new List<EventPackage>
                        {
                              new EventPackage { Id = ObjectId.GenerateNewId().ToString(), Title = "Soft Drinks Package", Price = 299},
                              new EventPackage { Id = ObjectId.GenerateNewId().ToString(), Title = "House Package", Price = 399}
                        },
                    }
                },
                Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.",
            };
        }

        public async Task GenerateOccurences()
        {
            var events = await _eventRepository.GetAll();

            foreach (var _event in events)
            {
                _event.Occurrences = GenerateListOfOccurences(10);
                await _eventRepository.Update(_event);
            }

        }

        private EventOccurrence GenerateOccurence()
        {
            Random random = new Random();
            var randomStartDay = random.Next(1, 30);
            var randomEndDay = randomStartDay > 27 ? randomStartDay : randomStartDay + 3;
            var randomStartHr = random.Next(9, 23);
            var randomEndHr = randomStartHr > 18 ? randomStartHr : randomStartHr + 5;


            return new EventOccurrence
            {
                Id = ObjectId.GenerateNewId().ToString(),
                StartDate = new DateTime(2021, 9, randomStartDay),
                EndDate = new DateTime(2021, 9, randomEndDay),
                StartTime = new TimeSpan(randomStartHr, 0, 0),
                EndTime = new TimeSpan(randomEndHr, 59, 59)
            };
        }

        private List<EventOccurrence> GenerateListOfOccurences(int size)
        {
            var list = new List<EventOccurrence>();
            for (int i = 0; i <= size; i++)
            {
                list.Add(GenerateOccurence());
            }

            return list;
        }
    }
}
