using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Data.Repositories;
using Common.Logic.Helpers;
using Common.Logic.Infrastructure.Interfaces;

namespace Common.Logic.Managers
{
    public class BaseManager : BaseHelper
    {
        
    }

    public abstract class BaseManager<TEntity> : BaseResult<TEntity>
    {
        public abstract Task<IOperationResult<ICollection<TEntity>>> SelectAsync(Func<TEntity, bool> predicate);
        public abstract Task<IOperationResult<TEntity>> FindAsync(Func<TEntity, bool> predicate);
        public abstract Task<IOperationResult<TEntity>> CreateAsync(TEntity item);
        public abstract Task<IOperationResult<TEntity>> UpdateAsync(TEntity item);
        public abstract Task<IOperationResult<bool>> RemoveAsync(string id);
        public abstract Task<IOperationResult<bool>> RemoveManyAsync(IEnumerable<string> ids);
        public abstract Task<IOperationResult<ICollection<TEntity>>> SelectAsync(Func<TEntity, bool> predicate, Func<TEntity, long> order, string include, int skip, int take);
        public abstract Task<IOperationResult<TEntity>> FindAsync(string include, Func<TEntity, bool> predicate);
    }

    public abstract class BaseManager<TEntity, TKey> : BaseManager<TEntity> where TEntity : class
    {
        protected readonly BaseRepository<TEntity, TKey> _repository;

        protected BaseManager(BaseRepository<TEntity, TKey> repository)
        {
            _repository = repository;
        }
    }
}
