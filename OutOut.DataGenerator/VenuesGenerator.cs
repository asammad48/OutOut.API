using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using OutOut.Constants.Enums;
using OutOut.Models.Models;
using Newtonsoft.Json;

namespace OutOut.DataGenerator
{
    public static class VenuesGenerator
    {
        public static void Generate()
        {
            while (true)
            {
                var venue = new Venue
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Logo = "9a51cffb-d60c-43c9-90c0-078ebd9434a6.jpg",
                    Background = "fa959ddd-2dd6-4b19-a573-e380ba5148f4.jpg",
                    Name = GenerateName(),
                    Description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.",
                    Location = new Location
                    {
                        GeoPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(GenerateLong(), GenerateLat())),
                        //City = GenerateCity(),
                        Area = "Area"
                    },
                    OpenTimes = new List<AvailableTime> {  new AvailableTime
                    {
                        Days = GenerateDaysOfWeek(),
                        From = new TimeSpan(GenerateTimeFrom(), 0, 0),
                        To = new TimeSpan(GenerateTimeTo(), 0, 0)
                    } },
                    Loyalty = new Loyalty
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        IsActive = true,
                        Stars = LoyaltyStars.FiveStars,
                        MaxUsage = MaxUsage.OncePerDay,
                        //Type = LoyaltyTypes.OneFreeHouseBeverage,
                        ValidOn = new List<AvailableTime>{new AvailableTime
                        {
                            Days = new List<DayOfWeek>() { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                            From = new TimeSpan(6, 0, 0),
                            To = new TimeSpan(23, 0, 0)
                        } },
                    },
                    LoyaltyCode = "4444",
                    Categories = new List<Category>
                    {
                        new Category
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            Icon = "Beach Club.png",
                            Name = "Beach Club",
                            IsActive = true,
                            TypeFor = 0
                        }
                    },
                    PhoneNumber = "+97148888888",
                    Menu = "71738897-31e2-4454-8eae-3914dd8218be.pdf",
                    SelectedTermsAndConditions = new List<string> { "60b8ba3e01536a0cb4bf2588", "60b8ba3e01536a0cb4bf2589" },
                    Gallery = new List<string> { "8a5ecc5f-58d2-4130-a600-ac89d488afb5.jpg", "951f7037-4175-4303-9ccc-32ee7c3ff77f.jpg" }
                };

                Console.WriteLine(JsonConvert.SerializeObject(venue));
                Console.ReadLine();
            }
        }

        private static string GenerateName()
        {
            var list = new List<string>
            {
                "Abu Dhabi National Exhibition Centre","Warehouse Four", "Emirates Living Events area", "Al Barsha Hall", "Al Jawaher Reception and Convention Centre", "Joharah Terrace", "Fort Island"
            };
            return Generate(list);
        }

        private static string GenerateCity()
        {
            var list = new List<string>
            {
                "Abu Dhabi","Ajman", "Dubai", "Fujairah", "Ras Al Khaimah", "Sharjah", "Umm Al Quwain"
            };
            return Generate(list);
        }

        private static double GenerateLong()
        {
            var list = new List<double>
            {
                55.715986,
                55.323854,
                54.674269,
                55.465813,
                55.201965,
                55.894566,
                55.553745,
                56.257339
            };
            return Generate(list);
        }

        private static double GenerateLat()
        {
            var list = new List<double>
            {
                24.429146,
                23.998297,
                23.394699,
                24.848559,
                23.616342,
                24.469651,
                24.968630,
                24.555118
            };
            return Generate(list);
        }

        private static List<DayOfWeek> GenerateDaysOfWeek()
        {
            var list = new List<List<DayOfWeek>>
            {
                new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday},
                new List<DayOfWeek>() { DayOfWeek.Saturday, DayOfWeek.Tuesday, DayOfWeek.Thursday}
            };
            return Generate(list);
        }

        private static int GenerateTimeFrom()
        {
            var list = new List<int>
            {
                0,1,2,3,4,5,6,7,8,9,10,11,12
            };
            return Generate(list);
        }

        private static int GenerateTimeTo()
        {
            var list = new List<int>
            {
                13,14,15,16,17,18,19,20,21,22,23
            };
            return Generate(list);
        }

        private static T Generate<T>(List<T> list)
        {
            Random rnd = new Random();
            int r = rnd.Next(list.Count);
            return list[r];
        }
    }
}
