using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// Extends the functionalities of a <see cref="IRepository"/> instance
	/// to provide a set of utility methods to perform common operations
	/// </summary>
    public static class RepositoryExtensions {
        private static IFilterableRepository RequireFilterable(this IRepository repository) {
            if (!(repository is IFilterableRepository filterable))
                throw new NotSupportedException("The repository is not filterable");

            return filterable;
        }

		private static ITransactionalRepository<TEntity> RequireTransactional<TEntity>(this IRepository<TEntity> repository)
			where TEntity : class {
			if (!(repository is ITransactionalRepository<TEntity> transactional))
				throw new NotSupportedException("The repository is not transactional");

			return transactional;
		}

        private static IFilterableRepository<TEntity> RequireFilterable<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class {
            if (!(repository is IFilterableRepository<TEntity> filterable))
                throw new NotSupportedException("The repository is not filterable");

            return filterable;
        }

		private static IQueryableRepository<TEntity> RequireQueryable<TEntity>(this IRepository<TEntity> repository)
			where TEntity : class {
			if (!(repository is IQueryableRepository<TEntity> queryable))
				throw new NotSupportedException("The repository is not queryable");

			return queryable;
		}

		private static bool IsFilterable<TEntity>(this IRepository<TEntity> repository) where TEntity : class {
			return repository is IFilterableRepository<TEntity>;
		}

		private static bool IsFilterable(this IRepository repository) {
			return repository is IFilterableRepository;
		}

		private static bool IsQueryable<TEntity>(this IRepository<TEntity> repository) where TEntity : class {
			return repository is IQueryableRepository<TEntity>;
		}

		#region AsFilterable

		public static IFilterableRepository<TEntity> AsFilterable<TEntity>(this IRepository<TEntity> repository)
			where TEntity : class {
			if (!(repository is IFilterableRepository<TEntity> filterable))
				throw new NotSupportedException("The repository is not filterable");

			return filterable;
		}

		#endregion


		#region Add

		/// <summary>
		/// Creates a new entity in the repository synchronously
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to create
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository to use to create the entity
		/// </param>
		/// <param name="entity">
		/// The instance of the entity to create
		/// </param>
		/// <returns>
		/// Returns a string that uniquely identifies the created entity
		/// within the underlying storage.
		/// </returns>
		public static string Add<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class
            => repository.AddAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

		/// <summary>
		/// Creates a new entity in the repository synchronously
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to create
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository to use to create the entity
		/// </param>
		/// <param name="transaction">
		/// A transaction to use to create the entity
		/// </param>
		/// <param name="entity">
		/// The instance of the entity to create
		/// </param>
		/// <returns>
		/// Returns a string that uniquely identifies the created entity
		/// within the underlying storage.
		/// </returns>
        public static string Add<TEntity>(this ITransactionalRepository<TEntity> repository, IDataTransaction transaction, TEntity entity)
            where TEntity : class
            => repository.RequireTransactional().AddAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

		/// <summary>
		/// Creates a new entity in the repository synchronously
		/// </summary>
		/// <param name="repository">
		/// The instance of the repository to use to create the entity
		/// </param>
		/// <param name="entity">
		/// The instance of the entity to create
		/// </param>
		/// <returns>
		/// Returns a string that uniquely identifies the created entity
		/// within the underlying storage.
		/// </returns>
        public static string Add(this IRepository repository, object entity)
            => repository.AddAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

		/// <summary>
		/// Creates a new entity in the repository synchronously
		/// within the given transaction
		/// </summary>
		/// <param name="repository">
		/// The instance of the repository to use to create the entity
		/// </param>
		/// <param name="transaction">
		/// The transaction scope to use to create the entity
		/// </param>
		/// <param name="entity">
		/// The instance of the entity to create
		/// </param>
		/// <returns>
		/// </returns>
        public static string Add(this ITransactionalRepository repository, IDataTransaction transaction, object entity)
            => repository.AddAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();


        #endregion

        #region Remove

        public static bool Remove<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class
            => repository.RemoveAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Remove<TEntity>(this ITransactionalRepository<TEntity> repository, IDataTransaction transaction, TEntity entity)
            where TEntity : class
            => repository.RequireTransactional().RemoveAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<bool> RemoveByIdAsync<TEntity>(this IRepository<TEntity> repository, string id, CancellationToken cancellationToken = default)
            where TEntity : class {
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.RemoveAsync(entity, cancellationToken);
        }

        public static async Task<bool> RemoveByIdAsync<TEntity>(this ITransactionalRepository<TEntity> repository, IDataTransaction transaction, string id, CancellationToken cancellationToken = default)
            where TEntity : class {
            // TODO: find within a transaction ...
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.RequireTransactional().RemoveAsync(transaction, entity, cancellationToken);
        }

        public static async Task<bool> RemoveByIdAsync(this IRepository repository, string id, CancellationToken cancellationToken = default) {
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.RemoveAsync(entity, cancellationToken);
        }

        public static async Task<bool> RemoveByIdAsync(this ITransactionalRepository repository, IDataTransaction transaction, string id, CancellationToken cancellationToken = default) {
            // TODO: find within a transaction ...
            var entity = await repository.FindByIdAsync(transaction, id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.RemoveAsync(transaction, entity, cancellationToken);
        }

        public static bool RemoveById<TEntity>(this IRepository<TEntity> repository, string id)
            where TEntity : class
            => repository.RemoveByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool RemoveById<TEntity>(this ITransactionalRepository<TEntity> repository, IDataTransaction transaction, string id)
            where TEntity : class
            => repository.RemoveByIdAsync(transaction, id).ConfigureAwait(false).GetAwaiter().GetResult();


        public static bool RemoveById(this IRepository repository, string id)
            => repository.RemoveByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool RemoveById(this ITransactionalRepository repository, IDataTransaction transaction, string id)
            => repository.RemoveByIdAsync(transaction, id).ConfigureAwait(false).GetAwaiter().GetResult();


        public static bool Remove(this IRepository repository, object entity)
            => repository.RemoveAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Remove(this ITransactionalRepository repository, IDataTransaction transaction, object entity)
            => repository.RemoveAsync(transaction, entity).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region Update

        public static bool Update<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class
            => repository.UpdateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Update(this IRepository repository, object entity)
            => repository.UpdateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region GetPage

        public static Task<RepositoryPage<TEntity>> GetPageAsync<TEntity>(this IPageableRepository<TEntity> repository, int page, int size, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.GetPageAsync(new RepositoryPageRequest<TEntity>(page, size), cancellationToken);

        //public static async Task<PaginatedResult<TDest>> GetPageAsync<TEntity, TDest>(this IRepository<TEntity> store, PageRequest page, CancellationToken cancellationToken = default)
        //	where TEntity : class
        //	where TDest : class {
        //	var result = await store.GetPageAsync(page, cancellationToken);
        //	return result.CastTo<TDest>();
        //}

        public static RepositoryPage<TEntity> GetPage<TEntity>(this IPageableRepository<TEntity> repository, RepositoryPageRequest<TEntity> request)
            where TEntity : class
            => repository.GetPageAsync(request).GetAwaiter().GetResult();

        #endregion

        #region Exists

        public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().ExistsAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static bool Exists<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class {
			if (repository.IsFilterable())
				return repository.RequireFilterable().Exists(filter);
			if (repository.IsQueryable())
				return repository.RequireQueryable().AsQueryable().Any(filter.AsLambda<TEntity>());

			throw new NotSupportedException("The repository does not support querying");
		}

        public static bool Exists(this IRepository repository, IQueryFilter filter)
            => repository.RequireFilterable().ExistsAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static bool Exists<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter)
            where TEntity : class
            => repository.ExistsAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region Count

        public static Task<long> CountAllAsync(this IRepository repository, CancellationToken cancellationToken = default)
            => repository.RequireFilterable().CountAsync(QueryFilter.Empty, cancellationToken);

        public static Task<long> CountAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class {
			if (repository.IsFilterable())
				return repository.RequireFilterable().CountAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);
			if (repository.IsQueryable())
				return Task.FromResult(repository.RequireQueryable().AsQueryable().LongCount(filter));

			throw new NotSupportedException("The repository does not support querying");
		}

        public static long Count<TEntity>(this IFilterableRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter)
            where TEntity : class
            => repository.CountAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static long Count(this IFilterableRepository repository, IQueryFilter filter)
            => repository.CountAsync(filter, default)
            .ConfigureAwait(false).GetAwaiter().GetResult();

        public static long CountAll(this IRepository repository)
            => repository.RequireFilterable().Count(QueryFilter.Empty);

        #endregion

        #region FindById

        public static TEntity? FindById<TEntity>(this IRepository<TEntity> store, string id)
            where TEntity : class
            => store.FindByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        public static object? FindById(this IRepository store, string id)
            => store.FindByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region FindFirst

        public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().FindAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().FindAsync(QueryFilter.Empty, cancellationToken);

        public static Task<object?> FindAsync(this IRepository repository, CancellationToken cancellationToken = default)
            => repository.RequireFilterable().FindAsync(QueryFilter.Empty, cancellationToken);

        public static TEntity? Find<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class
            => repository.RequireFilterable().FindAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static TEntity? Find<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class
            => repository.RequireFilterable().Find(QueryFilter.Empty);

        #endregion


        #region Find

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().FindAllAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().FindAllAsync(QueryFilter.Empty, cancellationToken);

        public static Task<IList<object>> FindAllAsync(this IRepository repository, CancellationToken cancellationToken = default)
            => repository.RequireFilterable().FindAllAsync(QueryFilter.Empty, cancellationToken);

        public static IList<TEntity> FindAll<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class
            => repository.RequireFilterable().FindAllAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static IList<TEntity> FindAll<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class
            => repository.FindAll(QueryFilter.Empty);

        public static IList<object> FindAll(this IRepository repository, IQueryFilter filter)
            => repository.RequireFilterable().FindAllAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static IList<object> FindAll(this IRepository repository)
            => repository.FindAll(QueryFilter.Empty);


        #endregion

        #region States

        public static void AddState<TEntity, TStatus>(this IStateRepository<TEntity, TStatus> repository, TEntity entity, EntityStateInfo<TStatus> stateInfo)
            where TEntity : class
            => repository.AddStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        public static void AddState<TStatus>(this IStateRepository<TStatus> repository, object entity, EntityStateInfo<TStatus> stateInfo)
            => repository.AddStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        public static void RemoveState<TEntity, TStatus>(this IStateRepository<TEntity, TStatus> repository, TEntity entity, EntityStateInfo<TStatus> stateInfo)
            where TEntity : class
            => repository.RemoveStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        public static void RemoveState<TStatus>(this IStateRepository<TStatus> repository, object entity, EntityStateInfo<TStatus> stateInfo)
            => repository.RemoveStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion
    }
}