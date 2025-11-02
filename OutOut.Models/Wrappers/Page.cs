using OutOut.Constants.Errors;
using OutOut.Models.Exceptions;
using MongoDB.Driver;

namespace OutOut.Models.Wrappers
{
    public class Page<T>
    {
        public int? NextPage { get; }
        public int PageNumber { get; }
        public int? PreviousPage { get; }
        public int PageSize { get; }
        public int RecordsTotalCount { get; }
        public int TotalPages { get; }
        public List<T> Records { get; }

        public Page(List<T> records, int pageNumber, int pageSize, long recordsTotalCount)
        {
            if (records.Count() == 0 && pageNumber > 0)
                throw new OutOutException(ErrorCodes.YouVeReachedLastPage);
            if (pageSize == 0)
                throw new OutOutException(ErrorCodes.PageSizeCannotBeZero);

            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.Records = records;
            this.RecordsTotalCount = (int)recordsTotalCount;

            this.TotalPages = (int)Math.Ceiling((decimal)recordsTotalCount / pageSize);

            this.NextPage = (recordsTotalCount > pageSize && pageNumber + 1 != this.TotalPages) ? pageNumber + 1 : (int?)null;
            this.PreviousPage = (pageNumber > 0) ? pageNumber - 1 : (int?)null;
        }

        public Page(List<T> records, int pageNumber, int pageSize)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.Records = records;
            this.RecordsTotalCount = 0;
        }

        public async static Task<Page<T>> CreateAsync(Task<List<T>> recordsTask, int pageNumber, int pageSize, Task<AggregateCountResult> recordsTotalCountTask)
        {
            var records = await recordsTask;
            var awaitedrecordsCount = await recordsTotalCountTask;
            var recordsCount = awaitedrecordsCount?.Count ?? 0;
            return new Page<T>(records, pageNumber, pageSize, recordsCount);
        }
        public async static Task<Page<T>> CreateAsync(Task<List<T>> recordsTask, int pageNumber, int pageSize, Task<List<T>> totalRecordsTask)
        {
            return new Page<T>(await recordsTask, pageNumber, pageSize,  (await totalRecordsTask).Count);
        }
    }
}
