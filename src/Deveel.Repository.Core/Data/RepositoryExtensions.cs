using System;
using System.Linq.Expressions;

using Deveel.States;

namespace Deveel.Data
{
    public static class RepositoryExtensions
    {

        #region Create

        public static string Create<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class, IEntity
            => repository.CreateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static string Create<TEntity>(this IRepository<TEntity> repository, IDataTransaction transaction, TEntity entity)
            where TEntity : class, IEntity
            => repository.CreateAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static string Create(this IRepository repository, IEntity entity)
            => repository.CreateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static string Create(this IRepository repository, IDataTransaction transaction, IEntity entity)
            => repository.CreateAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();


        #endregion

        #region Delete

        public static bool Delete<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class, IEntity
            => repository.DeleteAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Delete<TEntity>(this IRepository<TEntity> repository, IDataTransaction transaction, TEntity entity)
            where TEntity : class, IEntity
            => repository.DeleteAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<bool> DeleteByIdAsync<TEntity>(this IRepository<TEntity> repository, string id, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.DeleteAsync(entity, cancellationToken);
        }

        public static async Task<bool> DeleteByIdAsync<TEntity>(this IRepository<TEntity> repository, IDataTransaction transaction, string id, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            // TODO: find within a transaction ...
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.DeleteAsync(transaction, entity, cancellationToken);
        }

        public static async Task<bool> DeleteByIdAsync(this IRepository repository, string id, CancellationToken cancellationToken = default)
        {
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.DeleteAsync(entity, cancellationToken);
        }

        public static async Task<bool> DeleteByIdAsync(this IRepository repository, IDataTransaction transaction, string id, CancellationToken cancellationToken = default)
        {
            // TODO: find within a transaction ...
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.DeleteAsync(transaction, entity, cancellationToken);
        }

        public static bool DeleteById<TEntity>(this IRepository<TEntity> repository, string id)
            where TEntity : class, IEntity
            => repository.DeleteByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool DeleteById<TEntity>(this IRepository<TEntity> repository, IDataTransaction transaction, string id)
            where TEntity : class, IEntity
            => repository.DeleteByIdAsync(transaction, id).ConfigureAwait(false).GetAwaiter().GetResult();


        public static bool DeleteById(this IRepository repository, string id)
            => repository.DeleteByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool DeleteById(this IRepository repository, IDataTransaction transaction, string id)
            => repository.DeleteByIdAsync(transaction, id).ConfigureAwait(false).GetAwaiter().GetResult();


        public static bool Delete(this IRepository repository, IEntity entity)
            => repository.DeleteAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Delete(this IRepository repository, IDataTransaction transaction, IEntity entity)
            => repository.DeleteAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region Update

        public static bool Update<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class, IEntity
            => repository.UpdateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Update(this IRepository repository, IEntity entity)
            => repository.UpdateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region GetPage

        public static Task<RepositoryPage<TEntity>> GetPageAsync<TEntity>(this IRepository<TEntity> repository, int page, int size, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.GetPageAsync(new RepositoryPageRequest<TEntity>(page, size), cancellationToken);

        //public static async Task<PaginatedResult<TDest>> GetPageAsync<TEntity, TDest>(this IRepository<TEntity> store, PageRequest page, CancellationToken cancellationToken = default)
        //	where TEntity : class
        //	where TDest : class {
        //	var result = await store.GetPageAsync(page, cancellationToken);
        //	return result.CastTo<TDest>();
        //}

		public static RepositoryPage<TEntity> GetPage<TEntity>(this IRepository<TEntity> repository, RepositoryPageRequest<TEntity> request)
			where TEntity : class, IEntity
			=> repository.GetPageAsync(request).GetAwaiter().GetResult();

        #endregion

        #region Exists

        public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.ExistsAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static bool Exists<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class, IEntity
            => repository.ExistsAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Exists(this IRepository repository, IQueryFilter filter)
            => repository.ExistsAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Exists<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter)
            where TEntity : class, IEntity
            => repository.ExistsAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region Count

        public static Task<long> CountAllAsync(this IRepository repository, CancellationToken cancellationToken = default)
            => repository.CountAsync(QueryFilter.Empty, cancellationToken);

        public static Task<long> CountAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.CountAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static long Count<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter)
            where TEntity : class, IEntity
            => repository.CountAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static long Count(this IRepository repository, IQueryFilter filter)
            => repository.CountAsync(filter, default)
            .ConfigureAwait(false).GetAwaiter().GetResult();

        public static long CountAll(this IRepository store)
            => store.Count(QueryFilter.Empty);

        #endregion

        #region FindById

        public static TEntity? FindById<TEntity>(this IRepository<TEntity> store, string id)
            where TEntity : class, IEntity
            => store.FindByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        public static IEntity? FindById(this IRepository store, string id)
            => store.FindByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region FindFirst

        public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.FindAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.FindAsync(QueryFilter.Empty, cancellationToken);

        public static Task<IEntity?> FindAsync(this IRepository repository, CancellationToken cancellationToken = default)
            => repository.FindAsync(QueryFilter.Empty, cancellationToken);

        public static TEntity? Find<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class, IEntity
            => repository.FindAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static TEntity? Find<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class, IEntity
            => repository.Find(QueryFilter.Empty);

        #endregion


        #region Find

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.FindAllAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => repository.FindAllAsync(QueryFilter.Empty, cancellationToken);

        public static Task<IList<IEntity>> FindAllAsync(this IRepository repository, CancellationToken cancellationToken = default)
            => repository.FindAllAsync(QueryFilter.Empty, cancellationToken);

        public static IList<TEntity> FindAll<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class, IEntity
            => repository.FindAllAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static IList<TEntity> FindAll<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class, IEntity
            => repository.FindAll(QueryFilter.Empty);

        public static IList<IEntity> FindAll(this IRepository repository, IQueryFilter filter)
            => repository.FindAllAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static IList<IEntity> FindAll(this IRepository repository)
            => repository.FindAll(QueryFilter.Empty);


        #endregion

        #region States

        public static void AddState<TEntity, TStatus>(this IStateRepository<TEntity, TStatus> repository, TEntity entity, StateInfo<TStatus> stateInfo)
            where TEntity : class, IEntity
            => repository.AddStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        public static void AddState<TStatus>(this IStateRepository<TStatus> repository, IEntity entity, StateInfo<TStatus> stateInfo)
            => repository.AddStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        public static void RemoveState<TEntity, TStatus>(this IStateRepository<TEntity, TStatus> repository, TEntity entity, StateInfo<TStatus> stateInfo)
            where TEntity : class, IEntity
            => repository.RemoveStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        public static void RemoveState<TStatus>(this IStateRepository<TStatus> repository, IEntity entity, StateInfo<TStatus> stateInfo)
            => repository.RemoveStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion
    }
}
