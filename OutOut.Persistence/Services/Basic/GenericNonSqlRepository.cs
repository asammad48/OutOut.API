using MongoDB.Driver;
using OutOut.Models.EntityInterfaces;
using OutOut.Models.Models;
using OutOut.Models.Wrappers;
using OutOut.Persistence.Data;
using OutOut.Persistence.Extensions;
using OutOut.Persistence.Interfaces.Basic;
using OutOut.ViewModels.Wrappers;
using System.Linq.Expressions;

namespace OutOut.Persistence.Services.Basic
{
    public class GenericNonSqlRepository<TEntity> : IGenericNonSqlRepository<TEntity> where TEntity : INonSqlEntity
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        protected readonly ApplicationNonSqlDbContext _dbContext;
        protected readonly IEnumerable<ISyncRepository<TEntity>> _syncRepositories;
        protected IMongoCollection<TEntity> _collection
        {
            get { return _dbContext.GetCollection<TEntity>(); }
        }
        public GenericNonSqlRepository(ApplicationNonSqlDbContext dbContext, IEnumerable<ISyncRepository<TEntity>> syncRepositories)
        {
            _dbContext = dbContext;
            _syncRepositories = syncRepositories;
        }
        public virtual async Task<Page<TEntity>> GetPage(PaginationRequest pageRequest)
        {
            var query = _collection.Find<TEntity>(entity => true);

            var records = await query
                .Skip(pageRequest.PageNumber * pageRequest.PageSize)
                .Limit(pageRequest.PageSize)
                .ToListAsync();

            var recordsTotalCount = await query.CountDocumentsAsync();

            var page = new Page<TEntity>(records, pageRequest.PageNumber, pageRequest.PageSize, recordsTotalCount);
            return page;
        }

        public virtual async Task<Page<TEntity>> GetPageWhere(PaginationRequest pageRequest, FilterDefinition<TEntity> filterDef, SortDefinition<TEntity> sortDef = null)
        {
            var query = _collection.Find<TEntity>(filterDef);

            var records = await query
                .Skip(pageRequest.PageNumber * pageRequest.PageSize)
                .Limit(pageRequest.PageSize)
                .Sort(sortDef)
                .ToListAsync();

            var recordsTotalCount = await query.CountDocumentsAsync();

            var page = new Page<TEntity>(records, pageRequest.PageNumber, pageRequest.PageSize, recordsTotalCount);
            return page;
        }

        public virtual async Task<List<TEntity>> GetAll()
        {
            var result = await _collection.FindAsync<TEntity>(entity => true);
            return result.ToList();
        }
        public virtual async Task<List<TEntity>> Find(FilterDefinition<TEntity> filter)
        {
            var result = await _collection.FindAsync<TEntity>(filter);
            return result.ToList();
        }
        public virtual async Task<TEntity> FindFirst(FilterDefinition<TEntity> filter)
        {
            var query = _collection.Find<TEntity>(filter).Limit(1);

            var result = await query.FirstOrDefaultAsync();

            return result;
        }
        public virtual async Task<TEntity> GetById(string id)
        {
            FilterDefinition<TEntity> idFilter = Builders<TEntity>.Filter.Eq(c => c.Id, id);
            return await FindFirst(idFilter);
        }
        public virtual async Task<long> Count(FilterDefinition<TEntity> filters)
        {
            return await _collection.Find<TEntity>(filters).CountDocumentsAsync();
        }
        public virtual async Task<TEntity> Create(TEntity entity)
        {
            await _dbContext.GetCollection<TEntity>().InsertOneAsync(entity);
            return entity;
        }
        public virtual async Task<TEntity[]> CreateMany(TEntity[] entities)
        {
            await _collection.InsertManyAsync(entities);
            return entities;
        }
        public virtual async Task<TEntity> Update(TEntity entity)
        {
            TEntity oldEntity = null;
            if (_syncRepositories.Any())
            {
                oldEntity = await GetById(entity.Id);
            }
            await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
            if (_syncRepositories.Any())
            {
                await Sync(oldEntity, entity);
            }
            return entity;
        }


        public virtual async Task<bool> Delete(string id)
        {
            var result = await _collection.DeleteOneAsync(entity => entity.Id == id);
            return result.IsAcknowledged;
        }
        public virtual async Task<bool> DeleteMany(FilterDefinition<TEntity> filter)
        {
            var result = await _collection.DeleteManyAsync(filter);
            return result.IsAcknowledged;
        }

        public async Task Sync(TEntity oldEntity, TEntity entity)
        {
            foreach (var syncRepo in _syncRepositories)
            {
                await syncRepo.Sync(oldEntity, entity);
            }
        }

        public async Task<int> GetNextIncrementalNumber(string key)
        {
            var appState = await _dbContext.GetCollection<ApplicationState>().FirstOrDefaultAsync(i => i.Key == key);
            return int.Parse(appState?.Value ?? "0") + 1;
        }

        public async Task<int> GenerateLastIncrementalNumber(string key)
        {
            int? result = null;
            await semaphore.WaitAsync();
            try
            {
                var appState = await _dbContext.GetCollection<ApplicationState>().FirstOrDefaultAsync(i => i.Key == key);
                var lastIncrementalNumber = int.Parse(appState?.Value ?? "0");
                result = lastIncrementalNumber + 1;

                var updateIncrementalNumber = Builders<ApplicationState>.Update.Set(i => i.Value, result.ToString());
                var newApplicationState = new ApplicationState() { Key = key, Value = result.ToString() };
                if (appState == null)
                {
                    await _dbContext.GetCollection<ApplicationState>().InsertOneAsync(newApplicationState);
                }
                else
                {
                    newApplicationState.Id = appState.Id;
                    await _dbContext.GetCollection<ApplicationState>().ReplaceOneAsync(i => i.Key == key, newApplicationState);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                semaphore.Release();
            }

            return result ?? throw new InvalidOperationException();
        }

        public async Task<int> SubtractOneIncrementalNumber(string key)
        {
            int? result = null;
            await semaphore.WaitAsync();
            try
            {
                var appState = await _dbContext.GetCollection<ApplicationState>().FirstOrDefaultAsync(i => i.Key == key);
                var lastIncrementalNumber = int.Parse(appState?.Value ?? "0");
                result = lastIncrementalNumber - 1;

                var updateIncrementalNumber = Builders<ApplicationState>.Update.Set(i => i.Value, result.ToString());
                var newApplicationState = new ApplicationState() { Key = key, Value = result.ToString() };
                if (appState == null)
                {
                    await _dbContext.GetCollection<ApplicationState>().InsertOneAsync(newApplicationState);
                }
                else
                {
                    newApplicationState.Id = appState.Id;
                    await _dbContext.GetCollection<ApplicationState>().ReplaceOneAsync(i => i.Key == key, newApplicationState);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                semaphore.Release();
            }

            return result ?? throw new InvalidOperationException();
        }

        public async Task<bool> UpdateMany(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> updatedFields)
        {
            var result = await _collection.UpdateManyAsync(filter, updatedFields);

            return result.IsAcknowledged;
        }

        public async Task<BulkWriteResult<TEntity>> BulkUpdateDocuments<TKey, TValue>(Dictionary<TKey, TValue> entries,
                                                                                      Expression<Func<TEntity, TKey>> filterField,
                                                                                      ExpressionFieldDefinition<TEntity, TValue> willBeUpdatedField)
        {
            var bulkOps = new List<WriteModel<TEntity>>();
            foreach (var entry in entries.Keys)
            {
                var filter = Builders<TEntity>.Filter.Eq(filterField, entry);
                var updateDefinition = Builders<TEntity>.Update.Set(willBeUpdatedField, entries[entry]);
                var upsertOne = new UpdateOneModel<TEntity>(filter, updateDefinition);

                bulkOps.Add(upsertOne);
            }

            return await _collection.BulkWriteAsync(bulkOps);
        }

    }
}
