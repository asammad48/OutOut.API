using MongoDB.Driver;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Wrappers;

namespace OutOut.Persistence.Extensions
{
    public static class NonSqlPagination
    {
        public static Page<T> GetPaged<T>(this IEnumerable<T> list, PaginationRequest paginationRequest) where T : class
        {
            var skip = paginationRequest.PageNumber * paginationRequest.PageSize;
            var result = new Page<T>(list.Skip(skip).Take(paginationRequest.PageSize).ToList(), paginationRequest.PageNumber, paginationRequest.PageSize, list.Count());
            return result;
        }

        public static Task<Page<T>> GetPaged<T>(this IAggregateFluent<T> query, PaginationRequest paginationRequest) where T : class
        {
   
            var countTask = query.Count().FirstOrDefaultAsync();
            var recordsTask = query.Skip(paginationRequest.PageNumber * paginationRequest.PageSize)
                                     .Limit(paginationRequest.PageSize)
                                     .ToListAsync();

            return Page<T>.CreateAsync(recordsTask, paginationRequest.PageNumber, paginationRequest.PageSize, countTask);
        }

        public static Task<Page<T>> GetPaged<T>(this IFindFluent<T, T> query, PaginationRequest paginationRequest) where T : class
        {
            var totalRecords = query.ToListAsync();
            var recordsTask = query.Skip(paginationRequest.PageNumber * paginationRequest.PageSize)
                                     .Limit(paginationRequest.PageSize)
                                     .ToListAsync();

            return Page<T>.CreateAsync(recordsTask, paginationRequest.PageNumber, paginationRequest.PageSize, totalRecords);
        }

        public static Page<T> GetPaged<T>(this List<T> list, PaginationRequest paginationRequest) where T : class
        {
            var records = list.Skip(paginationRequest.PageNumber * paginationRequest.PageSize)
                                     .Take(paginationRequest.PageSize)
                                     .ToList();

            return new Page<T>(records, paginationRequest.PageNumber, paginationRequest.PageSize, list.Count());
        }

        public static List<T> Paginate<T>(this List<T> list, PaginationRequest paginationRequest) where T : class
        {
            return GetPaged(list, paginationRequest).Records;
        }
    }
}
