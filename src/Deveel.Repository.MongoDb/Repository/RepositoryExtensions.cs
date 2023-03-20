using MongoDB.Driver;

namespace Deveel.Data {
    public static class RepositoryExtensions {
        public static Task<TEntity?> FindAsync<TEntity>(this IFilterableRepository<TEntity> repository, FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.FindAsync(new MongoQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IFilterableRepository<TEntity> repository, FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.FindAllAsync(new MongoQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<long> CountAsync<TEntity>(this IFilterableRepository<TEntity> repository, FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.CountAsync(new MongoQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<bool> ExistsAsync<TEntity>(this IFilterableRepository<TEntity> repository, FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.ExistsAsync(new MongoQueryFilter<TEntity>(filter), cancellationToken);
    }
}