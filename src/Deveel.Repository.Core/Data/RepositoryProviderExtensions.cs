using System.Linq.Expressions;

namespace Deveel.Data {
    public static class RepositoryProviderExtensions {
        #region  Create

        public static Task<string> CreateAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).CreateAsync(entity, cancellationToken);

        public static Task<string> CreateAsync<TEntity>(this IRepositoryProvider<TEntity> provider, IDataTransaction transaction, string tenantId, TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).CreateAsync(transaction, entity, cancellationToken);

        public static string Create<TEntity>(this IRepositoryProvider<TEntity> provider, IDataTransaction transaction, string tenantId, TEntity entity)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).Create(transaction, entity);

        public static Task<string> CreateAsync(this IRepositoryProvider provider, string tenantId, IEntity entity, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).CreateAsync(entity, cancellationToken);

        public static string Create(this IRepositoryProvider provider, string tenantId, IEntity entity)
            => provider.GetRepository(tenantId).Create(entity);

        public static Task<string> CreateAsync(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, IEntity entity, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).CreateAsync(transaction, entity, cancellationToken);

        public static string Create(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, IEntity entity)
            => provider.GetRepository(tenantId).Create(transaction, entity);

        #endregion

        #region  Delete

        public static Task<bool> DeleteAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).DeleteAsync(entity, cancellationToken);

        public static Task<bool> DeleteAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).DeleteAsync(transaction, entity, cancellationToken);


        public static bool Delete<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, TEntity entity)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).Delete(entity);

        public static bool Delete<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, IDataTransaction transaction, TEntity entity)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).Delete(transaction, entity);


        public static Task<bool> DeleteAsync(this IRepositoryProvider provider, string tenantId, IEntity entity, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).DeleteAsync(entity, cancellationToken);

        public static Task<bool> DeleteAsync(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, IEntity entity, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).DeleteAsync(transaction, entity, cancellationToken);

        public static bool Delete(this IRepositoryProvider provider, string tenantId, IEntity entity)
            => provider.GetRepository(tenantId).Delete(entity);

        public static bool Delete(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, IEntity entity)
            => provider.GetRepository(tenantId).Delete(transaction, entity);

        public static Task<bool> DeleteByIdAsync(this IRepositoryProvider provider, string tenantId, string id, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).DeleteByIdAsync(id, cancellationToken);

        public static Task<bool> DeleteByIdAsync(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, string id, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).DeleteByIdAsync(transaction, id, cancellationToken);


        public static bool DeleteById(this IRepositoryProvider provider, string tenantId, string id)
            => provider.GetRepository(tenantId).DeleteById(id);

        public static bool DeleteById(this IRepositoryProvider provider, string tenantId, IDataTransaction transaction, string id)
            => provider.GetRepository(tenantId).DeleteById(transaction, id);


        #endregion

        #region Find

        public static Task<TEntity?> FindAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).FindAsync(filter, cancellationToken);

        public static Task<TEntity?> FindAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).FindAsync(filter, cancellationToken);

        public static Task<TEntity?> FindAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).FindAsync(cancellationToken);

        public static Task<IEntity?> FindAsync(this IRepositoryProvider provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).FindAsync(filter, cancellationToken);

        public static Task<IEntity?> FindAsync(this IRepositoryProvider provider, string tenantId, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).FindAsync(cancellationToken);

        #endregion

        #region FindAll

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).FindAllAsync(filter, cancellationToken);

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).FindAllAsync(filter, cancellationToken);

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => provider.GetRepository(tenantId).FindAllAsync(cancellationToken);

        public static Task<IList<IEntity>> FindAllAsync(this IRepositoryProvider provider, string tenantId, IQueryFilter filter, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).FindAllAsync(filter, cancellationToken);

        public static Task<IList<IEntity>> FindAllAsync(this IRepositoryProvider provider, string tenantId, CancellationToken cancellationToken = default)
            => provider.GetRepository(tenantId).FindAllAsync(cancellationToken);

        #endregion
    }
}