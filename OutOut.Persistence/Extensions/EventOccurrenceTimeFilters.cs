using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using OutOut.Models.Models;
using OutOut.Models.Utils;
using System.Linq.Expressions;

namespace OutOut.Persistence.Extensions
{
    public static class EventOccurrenceTimeFilters
    {
        public static FilterDefinition<T> IsAvailableToday<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, EventOccurrence>> field)
        {
            var fieldDef = new ExpressionFieldDefinition<T, EventOccurrence>(field);
            var fieldName = fieldDef.Render(BsonSerializer.SerializerRegistry.GetSerializer<T>(), BsonSerializer.SerializerRegistry);
            var startDateFieldName = fieldName.FieldName + ".StartDate";
            var startTimeFieldName = fieldName.FieldName + ".StartTime";
            return builder.Eq(startDateFieldName, UAEDateTime.Now.Date) &
                   builder.Gte(startTimeFieldName, UAEDateTime.Now.TimeOfDay);
        }

        public static FilterDefinition<T> OngoingEvent<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, EventOccurrence>> field)
        {
            var fieldDef = new ExpressionFieldDefinition<T, EventOccurrence>(field);
            var fieldName = fieldDef.Render(BsonSerializer.SerializerRegistry.GetSerializer<T>(), BsonSerializer.SerializerRegistry);

            var startDateFieldName = fieldName.FieldName + ".StartDate";
            var endDateFieldName = fieldName.FieldName + ".EndDate";

            return builder.Lte(startDateFieldName, UAEDateTime.Now.Date) &
                   builder.Gte(endDateFieldName, UAEDateTime.Now.Date.AddDays(-1));
        }

        public static FilterDefinition<T> UpcomingDate<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, EventOccurrence>> field)
        {
            var fieldDef = new ExpressionFieldDefinition<T, EventOccurrence>(field);
            var fieldName = fieldDef.Render(BsonSerializer.SerializerRegistry.GetSerializer<T>(), BsonSerializer.SerializerRegistry);

            var startDateFieldName = fieldName.FieldName + ".StartDate";
            var startTimeFieldName = fieldName.FieldName + ".StartTime";

            return builder.Gt(startDateFieldName, UAEDateTime.Now.Date) |
                  (builder.Gte(startTimeFieldName, UAEDateTime.Now.TimeOfDay) &
                   builder.Eq(startDateFieldName, UAEDateTime.Now.Date));
        }

        public static FilterDefinition<T> HistoryBooking<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, EventOccurrence>> field)
        {
            var fieldDef = new ExpressionFieldDefinition<T, EventOccurrence>(field);
            var fieldName = fieldDef.Render(BsonSerializer.SerializerRegistry.GetSerializer<T>(), BsonSerializer.SerializerRegistry);

            var endDateFieldName = fieldName.FieldName + ".EndDate";
            var endTimeFieldName = fieldName.FieldName + ".EndTime";

            return builder.Lt(endDateFieldName, UAEDateTime.Now.Date.AddDays(-1)) |
                   (builder.Eq(endDateFieldName, UAEDateTime.Now.Date.AddDays(-1)) &
                    builder.Lt(endTimeFieldName, UAEDateTime.Now.AddHours(-24).TimeOfDay));
        }

        public static FilterDefinition<T> RecentBooking<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, EventOccurrence>> field)
        {
            var fieldDef = new ExpressionFieldDefinition<T, EventOccurrence>(field);
            var fieldName = fieldDef.Render(BsonSerializer.SerializerRegistry.GetSerializer<T>(), BsonSerializer.SerializerRegistry);

            var endDateFieldName = fieldName.FieldName + ".EndDate";
            var endTimeFieldName = fieldName.FieldName + ".EndTime";

            return builder.Gt(endDateFieldName, UAEDateTime.Now.Date.AddDays(-1)) |
                   (builder.Eq(endDateFieldName, UAEDateTime.Now.Date.AddDays(-1)) &
                    builder.Gte(endTimeFieldName, UAEDateTime.Now.AddHours(-24).TimeOfDay));
        }

        public static FilterDefinition<T> IsEventAvailableInRange<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, EventOccurrence>> field, DateTime fromDateTime, DateTime toDateTime)
        {
            var fieldDef = new ExpressionFieldDefinition<T, EventOccurrence>(field);
            var fieldName = fieldDef.Render(BsonSerializer.SerializerRegistry.GetSerializer<T>(), BsonSerializer.SerializerRegistry);

            var startDateFieldName = fieldName.FieldName + ".StartDate";
            var startTimeFieldName = fieldName.FieldName + ".StartTime";

            if ((toDateTime.Day - fromDateTime.Day) == 1 || toDateTime == fromDateTime)
                return builder.Eq(startDateFieldName, fromDateTime.Date) &
                       builder.Gte(startTimeFieldName, fromDateTime.TimeOfDay);

            else
                return (builder.Gt(startDateFieldName, fromDateTime.Date) |
                  (builder.Eq(startDateFieldName, fromDateTime.Date) &
                   builder.Gte(startTimeFieldName, fromDateTime.TimeOfDay))) &

                  (builder.Lt(startDateFieldName, toDateTime.Date) |
                  (builder.Eq(startDateFieldName, toDateTime.Date) &
                   builder.Lte(startTimeFieldName, toDateTime.TimeOfDay)));
        }

        public static FilterDefinition<T> AllUpcomingEvents<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, EventOccurrence>> field, DateTime fromDateTime)
        {
            var fieldDef = new ExpressionFieldDefinition<T, EventOccurrence>(field);
            var fieldName = fieldDef.Render(BsonSerializer.SerializerRegistry.GetSerializer<T>(), BsonSerializer.SerializerRegistry);

            var startDateFieldName = fieldName.FieldName + ".StartDate";
            var startTimeFieldName = fieldName.FieldName + ".StartTime";

            return builder.Gt(startDateFieldName, fromDateTime.Date) |
                  (builder.Eq(startDateFieldName, fromDateTime.Date) &
                   builder.Gte(startTimeFieldName, fromDateTime.TimeOfDay));
        }

        public static FilterDefinition<T> PackageFilter<T>(this FilterDefinitionBuilder<T> builder, Expression<Func<T, IEnumerable<EventOccurrence>>> field, string occurrenceId, string packageId)
        {
            var def = Builders<EventOccurrence>.Filter.Eq("Id", new BsonObjectId(new ObjectId(occurrenceId))) &
                      Builders<EventOccurrence>.Filter.ElemMatch("Packages", Builders<EventPackage>.Filter.Eq("Id", new BsonObjectId(new ObjectId(packageId))));
            return builder.ElemMatch(field, def);
        }

        public static SortDefinition<EventBooking> GetAscendingDateTimeSort() =>
            Builders<EventBooking>.Sort.Ascending("Event.Occurrence.StartDate").Ascending("Event.Occurrence.StartTime").Ascending("Event.Name");

        public static SortDefinition<EventBooking> GetDescendingDateTimeSort() =>
            Builders<EventBooking>.Sort.Descending("Event.Occurrence.StartDate").Descending("Event.Occurrence.StartTime").Ascending("Event.Name");

        public static SortDefinition<T> GetAscendingDateTimeSort<T>() =>
          Builders<T>.Sort.Ascending("Occurrence.StartDate").Ascending("Occurrence.StartTime").Ascending("Name");

        public static SortDefinition<T> GetDescendingDateTimeSort<T>() =>
            Builders<T>.Sort.Descending("Occurrence.StartDate").Descending("Occurrence.StartTime").Ascending("Name");

    }
}
