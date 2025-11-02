using MongoDB.Driver;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Wrappers;
using OutOut.ViewModels.Wrappers;
using System.Linq.Expressions;

namespace OutOut.Persistence.Interfaces.Basic
{
    public interface IGenericNonSqlRepository<TEntity> where TEntity : INonSqlEntity
    {
        Task<Page<TEntity>> GetPage(PaginationRequest pageRequest);
        Task<List<TEntity>> GetAll();
        Task<List<TEntity>> Find(FilterDefinition<TEntity> filter);
        Task<TEntity> FindFirst(FilterDefinition<TEntity> filter);
        Task<TEntity> GetById(string id);
        Task<TEntity> Create(TEntity entity);
        Task<TEntity[]> CreateMany(TEntity[] entities);
        Task<TEntity> Update(TEntity entity);
        Task<bool> Delete(string id);
        Task<bool> DeleteMany(FilterDefinition<TEntity> filter);
        Task<long> Count(FilterDefinition<TEntity> filters);
        Task<bool> UpdateMany(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> updatedFields);

        Task Sync(TEntity oldEntity, TEntity entity);

        Task<int> GetNextIncrementalNumber(string key);
        Task<int> GenerateLastIncrementalNumber(string key);
        Task<int> SubtractOneIncrementalNumber(string key);

        Task<BulkWriteResult<TEntity>> BulkUpdateDocuments<TKey, TValue>(
             Dictionary<TKey, TValue> entries,
             Expression<Func<TEntity, TKey>> filterField,
             ExpressionFieldDefinition<TEntity, TValue> willBeUpdatedField);
    }
}
