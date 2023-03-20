using System;
using System.Linq.Expressions;

namespace Deveel.Data {
    public static class RepositoryExtensions {
        private static IFilterableRepository RequireFilterable(this IRepository repository) {
            if (!(repository is IFilterableRepository filterable))
                throw new NotSupportedException("The repository is not filterable");

            return filterable;
        }

        private static IFilterableRepository<TEntity> RequireFilterable<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class, IDataEntity {
            if (!(repository is IFilterableRepository<TEntity> filterable))
                throw new NotSupportedException("The repository is not filterable");

            return filterable;
        }


        #region Create

        public static string Create<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class, IDataEntity
            => repository.CreateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static string Create<TEntity>(this IRepository<TEntity> repository, IDataTransaction transaction, TEntity entity)
            where TEntity : class, IDataEntity
            => repository.CreateAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static string Create(this IRepository repository, IDataEntity entity)
            => repository.CreateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static string Create(this IRepository repository, IDataTransaction transaction, IDataEntity entity)
            => repository.CreateAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();


        #endregion

        #region Delete

        public static bool Delete<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class, IDataEntity
            => repository.DeleteAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Delete<TEntity>(this IRepository<TEntity> repository, IDataTransaction transaction, TEntity entity)
            where TEntity : class, IDataEntity
            => repository.DeleteAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<bool> DeleteByIdAsync<TEntity>(this IRepository<TEntity> repository, string id, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity {
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.DeleteAsync(entity, cancellationToken);
        }

        public static async Task<bool> DeleteByIdAsync<TEntity>(this IRepository<TEntity> repository, IDataTransaction transaction, string id, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity {
            // TODO: find within a transaction ...
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.DeleteAsync(transaction, entity, cancellationToken);
        }

        public static async Task<bool> DeleteByIdAsync(this IRepository repository, string id, CancellationToken cancellationToken = default) {
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.DeleteAsync(entity, cancellationToken);
        }

        public static async Task<bool> DeleteByIdAsync(this IRepository repository, IDataTransaction transaction, string id, CancellationToken cancellationToken = default) {
            // TODO: find within a transaction ...
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.DeleteAsync(transaction, entity, cancellationToken);
        }

        public static bool DeleteById<TEntity>(this IRepository<TEntity> repository, string id)
            where TEntity : class, IDataEntity
            => repository.DeleteByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool DeleteById<TEntity>(this IRepository<TEntity> repository, IDataTransaction transaction, string id)
            where TEntity : class, IDataEntity
            => repository.DeleteByIdAsync(transaction, id).ConfigureAwait(false).GetAwaiter().GetResult();


        public static bool DeleteById(this IRepository repository, string id)
            => repository.DeleteByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool DeleteById(this IRepository repository, IDataTransaction transaction, string id)
            => repository.DeleteByIdAsync(transaction, id).ConfigureAwait(false).GetAwaiter().GetResult();


        public static bool Delete(this IRepository repository, IDataEntity entity)
            => repository.DeleteAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Delete(this IRepository repository, IDataTransaction transaction, IDataEntity entity)
            => repository.DeleteAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region Update

        public static bool Update<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class, IDataEntity
            => repository.UpdateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Update(this IRepository repository, IDataEntity entity)
            => repository.UpdateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region GetPage

        public static Task<RepositoryPage<TEntity>> GetPageAsync<TEntity>(this IPageableRepository<TEntity> repository, int page, int size, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.GetPageAsync(new RepositoryPageRequest<TEntity>(page, size), cancellationToken);

        //public static async Task<PaginatedResult<TDest>> GetPageAsync<TEntity, TDest>(this IRepository<TEntity> store, PageRequest page, CancellationToken cancellationToken = default)
        //	where TEntity : class
        //	where TDest : class {
        //	var result = await store.GetPageAsync(page, cancellationToken);
        //	return result.CastTo<TDest>();
        //}

        public static RepositoryPage<TEntity> GetPage<TEntity>(this IPageableRepository<TEntity> repository, RepositoryPageRequest<TEntity> request)
            where TEntity : class, IDataEntity
            => repository.GetPageAsync(request).GetAwaiter().GetResult();

        #endregion

        #region Exists

        public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.RequireFilterable().ExistsAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static bool Exists<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class, IDataEntity
            => repository.RequireFilterable().ExistsAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Exists(this IRepository repository, IQueryFilter filter)
            => repository.RequireFilterable().ExistsAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Exists<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter)
            where TEntity : class, IDataEntity
            => repository.ExistsAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region Count

        public static Task<long> CountAllAsync(this IFilterableRepository repository, CancellationToken cancellationToken = default)
            => repository.CountAsync(QueryFilter.Empty, cancellationToken);

        public static Task<long> CountAsync<TEntity>(this IFilterableRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.CountAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static long Count<TEntity>(this IFilterableRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter)
            where TEntity : class, IDataEntity
            => repository.CountAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static long Count(this IFilterableRepository repository, IQueryFilter filter)
            => repository.CountAsync(filter, default)
            .ConfigureAwait(false).GetAwaiter().GetResult();

        public static long CountAll(this IRepository repository)
            => repository.RequireFilterable().Count(QueryFilter.Empty);

        #endregion

        #region FindById

        public static TEntity? FindById<TEntity>(this IRepository<TEntity> store, string id)
            where TEntity : class, IDataEntity
            => store.FindByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        public static IDataEntity? FindById(this IRepository store, string id)
            => store.FindByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region FindFirst

        public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.RequireFilterable().FindAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.RequireFilterable().FindAsync(QueryFilter.Empty, cancellationToken);

        public static Task<IDataEntity?> FindAsync(this IRepository repository, CancellationToken cancellationToken = default)
            => repository.RequireFilterable().FindAsync(QueryFilter.Empty, cancellationToken);

        public static TEntity? Find<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class, IDataEntity
            => repository.RequireFilterable().FindAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static TEntity? Find<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class, IDataEntity
            => repository.RequireFilterable().Find(QueryFilter.Empty);

        #endregion


        #region Find

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.RequireFilterable().FindAllAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
            where TEntity : class, IDataEntity
            => repository.RequireFilterable().FindAllAsync(QueryFilter.Empty, cancellationToken);

        public static Task<IList<IDataEntity>> FindAllAsync(this IRepository repository, CancellationToken cancellationToken = default)
            => repository.RequireFilterable().FindAllAsync(QueryFilter.Empty, cancellationToken);

        public static IList<TEntity> FindAll<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class, IDataEntity
            => repository.RequireFilterable().FindAllAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static IList<TEntity> FindAll<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class, IDataEntity
            => repository.FindAll(QueryFilter.Empty);

        public static IList<IDataEntity> FindAll(this IRepository repository, IQueryFilter filter)
            => repository.RequireFilterable().FindAllAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static IList<IDataEntity> FindAll(this IRepository repository)
            => repository.FindAll(QueryFilter.Empty);


        #endregion

        #region States

        public static void AddState<TEntity, TStatus>(this IStateRepository<TEntity, TStatus> repository, TEntity entity, EntityStateInfo<TStatus> stateInfo)
            where TEntity : class, IDataEntity
            => repository.AddStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        public static void AddState<TStatus>(this IStateRepository<TStatus> repository, IDataEntity entity, EntityStateInfo<TStatus> stateInfo)
            => repository.AddStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        public static void RemoveState<TEntity, TStatus>(this IStateRepository<TEntity, TStatus> repository, TEntity entity, EntityStateInfo<TStatus> stateInfo)
            where TEntity : class, IDataEntity
            => repository.RemoveStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        public static void RemoveState<TStatus>(this IStateRepository<TStatus> repository, IDataEntity entity, EntityStateInfo<TStatus> stateInfo)
            => repository.RemoveStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion
    }
}