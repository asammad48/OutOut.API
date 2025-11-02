using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Models.Utils;
using System.Linq.Expressions;

namespace OutOut.Persistence.Extensions
{
    public static class AvailableTimeFilters
    {
        public static FilterDefinition<AvailableTime> IsCurrentlyAvailable()
        {
            return IsAvailableAt(UAEDateTime.Now);
        }

        public static FilterDefinition<AvailableTime> IsAvailableAt(DateTime dateTime)
        {
            return Builders<AvailableTime>.Filter.AnyEq("Days", dateTime.DayOfWeek) &
                   Builders<AvailableTime>.Filter.Lte("From", dateTime.TimeOfDay) &
                   Builders<AvailableTime>.Filter.Gte("To", dateTime.TimeOfDay);
        }

        public static FilterDefinition<T> IsAvailableInRange<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, IEnumerable<AvailableTime>>> field, DateTime fromDateTime, DateTime toDateTime)
        {
            var difference = Enumerable.Range(0, (toDateTime - fromDateTime).Days + 1).Select(d => fromDateTime.AddDays(d)).Select(a => a.DayOfWeek).ToList();

            var filterDef = Builders<AvailableTime>.Filter.Empty;
            if ((toDateTime.Day - fromDateTime.Day) == 1 || toDateTime == fromDateTime)
                filterDef = Builders<AvailableTime>.Filter.AnyIn("Days", new List<DayOfWeek> { fromDateTime.DayOfWeek}) &
                            Builders<AvailableTime>.Filter.Gte("From", fromDateTime.TimeOfDay);

            else
                filterDef = Builders<AvailableTime>.Filter.AnyIn("Days", difference.SkipLast(1)) &
                            Builders<AvailableTime>.Filter.Gte("From", toDateTime.TimeOfDay);

            return builder.ElemMatch(field, filterDef);
        }
        public static FilterDefinition<T> IsAvailableInRangeDateOnly<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, IEnumerable<AvailableTime>>> field, DateTime fromDateTime, DateTime toDateTime)
        {
            var difference = Enumerable.Range(0, (toDateTime - fromDateTime).Days + 1).Select(d => fromDateTime.AddDays(d)).Select(a => a.DayOfWeek).ToList();

            var filterDef = Builders<AvailableTime>.Filter.Empty;
            if ((toDateTime.Day - fromDateTime.Day) == 1 || toDateTime == fromDateTime)
                filterDef = Builders<AvailableTime>.Filter.AnyIn("Days", new List<DayOfWeek> { fromDateTime.DayOfWeek });
            else
                filterDef = Builders<AvailableTime>.Filter.AnyIn("Days", difference.SkipLast(1));

            return builder.ElemMatch(field, filterDef);
        }
    }
}
