using System;

namespace Deveel.Data
{
    public interface IQueryableRepository<TEntity> : IRepository<TEntity> where TEntity : class, IDataEntity
    {
        IQueryable<TEntity> AsQueryable();
    }
}
