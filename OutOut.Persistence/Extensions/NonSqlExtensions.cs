using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace OutOut.Persistence.Extensions
{
    public static class NonSqlExtensions
    {
        private static FindOptions<TItem> LimitOneOption<TItem>() => new FindOptions<TItem>
        {
            Limit = 1
        };
        public static FilterDefinition<TItem> SearchContains<TItem>(this FilterDefinitionBuilder<TItem> builder, Expression<Func<TItem, object>> field, string Text)
        {
            string queryText = Regex.Escape(Text);
            return builder.Regex(field, new Regex(queryText, RegexOptions.IgnoreCase));
        }
        public static async Task<TItem> FirstOrDefaultAsync<TItem>(this IMongoCollection<TItem> mongoCollection, Expression<Func<TItem, bool>> p)
        {
            return await (await mongoCollection.FindAsync(p, LimitOneOption<TItem>())).FirstOrDefaultAsync();
        }

        public static FilterDefinition<TItem> InOrParameterEmpty<TItem>(this FilterDefinitionBuilder<TItem> builder, Expression<Func<TItem, string>> field, List<string> list, bool isSuperAdmin)
        {
            if (isSuperAdmin)
                return builder.Empty;

            return builder.In(field, list);
        }

        public static FilterDefinition<TItem> ObjectIdEq<TItem>(this FilterDefinitionBuilder<TItem> builder, string field, string id)
        {
            return builder.Eq(field, new BsonObjectId(new ObjectId(id)));
        }

        public static FilterDefinition<TItem> ObjectIdIn<TItem>(this FilterDefinitionBuilder<TItem> builder, string field, List<string> ids)
        {
            var objectIdList = new List<BsonObjectId>();
            foreach (var id in ids)
            {
                objectIdList.Add(new BsonObjectId(new ObjectId(id)));
            }
            return builder.In(field, objectIdList);
        }

        public static async Task ParallelForEachAsync<TItem>(this IMongoCollection<TItem> mongoCollection, FilterDefinition<TItem> filter, Func<TItem, Task> process, int batchSize = 100)
        {
            var options = new FindOptions<TItem> { BatchSize = batchSize, };
            using var cursor = await mongoCollection.FindAsync(filter, options);
            while (cursor.MoveNext())
            {
                var batch = cursor.Current.ToList();
                var batchTasks = batch.Select(item => process(item));
                try
                {
                    await Task.WhenAll(batchTasks);
                }
                catch { }
            }
        }
    }
}
