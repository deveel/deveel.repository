using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// Extends the functionalities of a repository instance
	/// to provide a set of utility methods to perform common operations
	/// </summary>
	public static class RepositoryExtensions {
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

		private static bool IsQueryable<TEntity>(this IRepository<TEntity> repository) where TEntity : class {
			return repository is IQueryableRepository<TEntity>;
		}

		private static bool IsPageable<TEntity>(this IRepository<TEntity> repository) where TEntity : class {
			return repository is IPageableRepository<TEntity>;
		}

		private static IPageableRepository<TEntity> RequirePageable<TEntity>(this IRepository<TEntity> repository) where TEntity : class {
			if (!(repository is IPageableRepository<TEntity> pageable))
				throw new NotSupportedException("The repository is not pageable");

			return pageable;
		}

		#region AsFilterable

		/// <summary>
		/// Gets a version of the repository that is filterable
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to filter
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository to get the filterable version.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="IFilterableRepository{TEntity}"/>
		/// that can be used to filter the entities in the repository.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository is not filterable
		/// </exception>
		public static IFilterableRepository<TEntity> AsFilterable<TEntity>(this IRepository<TEntity> repository)
			where TEntity : class {
			if (!(repository is IFilterableRepository<TEntity> filterable))
				throw new NotSupportedException("The repository is not filterable");

			// TODO: If the repository is a queryable, we can wrap it
			//        in a filterable version, so that we can use the filter

			return filterable;
		}

		#endregion


		#region Add

		/// <summary>
		/// Adds a new entity in the repository synchronously
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to add
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

        #endregion

        #region Remove

		/// <summary>
		/// Removes an entity from the repository synchronously
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository from which the entity is removed
		/// </param>
		/// <param name="entity">
		/// The instance of the entity to remove
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the entity was removed successfully,
		/// otherwise <c>false</c>.
		/// </returns>
		/// <seealso cref="IRepository{TEntity}.RemoveAsync(TEntity, CancellationToken)"/>
        public static bool Remove<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class
            => repository.RemoveAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

		/// <summary>
		/// Removes an entity, identified by the given key,
		/// from the repository
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository from which the entity is removed
		/// </param>
		/// <param name="id">
		/// The string that uniquely identifies the entity to remove
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the entity was removed successfully,
		/// otherwise it returns <c>false</c>.
		/// </returns>
        public static async Task<bool> RemoveByIdAsync<TEntity>(this IRepository<TEntity> repository, string id, CancellationToken cancellationToken = default)
            where TEntity : class {
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            return await repository.RemoveAsync(entity, cancellationToken);
        }

		/// <summary>
		/// Synchronously removes an entity, identified by the given key,
		/// from the repository
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository from which the entity is removed.
		/// </param>
		/// <param name="id">
		/// The string that uniquely identifies the entity to remove.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the entity was removed successfully,
		/// otherwise it returns <c>false</c>.
		/// </returns>
		/// <seealso cref="IRepository{TEntity}.RemoveAsync(TEntity, CancellationToken)"/>
        public static bool RemoveById<TEntity>(this IRepository<TEntity> repository, string id)
            where TEntity : class
            => repository.RemoveByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region Update

		/// <summary>
		/// Updates an entity in the repository synchronously
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository from which the entity is updated
		/// </param>
		/// <param name="entity">
		/// The instance of the entity to update
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the entity was updated successfully,
		/// otherwise <c>false</c>.
		/// </returns>
        public static bool Update<TEntity>(this IRepository<TEntity> repository, TEntity entity)
            where TEntity : class
            => repository.UpdateAsync(entity).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region GetPage

		/// <summary>
		/// Gets a page of entities from the repository,
		/// given a request object that defines the scope
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="repository"></param>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <remarks>
		/// <para>
		/// This method attempts to cast the given repository to a
		/// <see cref="IPageableRepository{TEntity}"/> and invoke the
		/// native method <see cref="IPageableRepository{TEntity}.GetPageAsync(RepositoryPageRequest{TEntity}, CancellationToken)"/>.
		/// </para>
		/// <para>
		/// If the repository does not implement the interface, the method
		/// attempts to cast it to a <see cref="IQueryableRepository{TEntity}"/>
		/// and invoke the a paging operation on the <see cref="IQueryable{T}"/>.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="RepositoryPage{TEntity}"/> that
		/// represents the result of the query.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support paging.
		/// </exception>
		public static Task<RepositoryPage<TEntity>> GetPageAsync<TEntity>(this IRepository<TEntity> repository, RepositoryPageRequest<TEntity> request, CancellationToken cancellationToken = default)
			where TEntity : class {
			if (repository.IsPageable())
				return repository.RequirePageable().GetPageAsync(request, cancellationToken);
			if (repository.IsQueryable())
				return Task.FromResult(repository.RequireQueryable().GetPage(request));

			throw new NotSupportedException("The repository does not support paging");
		}

		#endregion

		#region Exists

		/// <summary>
		/// Checks if an entity exists in the repository,
		/// that matches the given filter
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository to use to check the existence 
		/// of any entity that matches the given filter.
		/// </param>
		/// <param name="filter">
		/// The filtering expression to use to check the existence of
		/// any matching entity.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if any entity exists in the repository
		/// that matches the given filter, otherwise <c>false</c>.
		/// </returns>
		public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().ExistsAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

		/// <summary>
		/// Synchronously checks if an entity exists in the repository,
		/// that matches the given filter
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository to use to check the existence
		/// of any entity that matches the given filter.
		/// </param>
		/// <param name="filter">
		/// The filter used to check the existence of any matching entity.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if any entity exists in the repository
		/// that matches the given filter, otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support querying or filtering.
		/// </exception>
        public static bool Exists<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class {
			if (repository.IsFilterable())
				return repository.RequireFilterable().Exists(filter);
			if (repository.IsQueryable())
				return repository.RequireQueryable().AsQueryable().Any(filter.AsLambda<TEntity>());

			throw new NotSupportedException("The repository does not support querying");
		}

        public static bool Exists<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter)
            where TEntity : class
            => repository.ExistsAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region Count

        public static Task<long> CountAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class {
			if (repository.IsFilterable())
				return repository.RequireFilterable().CountAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);
			if (repository.IsQueryable())
				return Task.FromResult(repository.RequireQueryable().AsQueryable().LongCount(filter));

			throw new NotSupportedException("The repository does not support querying");
		}

		public static Task<long> CountAllAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
			where TEntity : class {
			if (repository.IsFilterable())
				return repository.RequireFilterable().CountAsync(QueryFilter.Empty, cancellationToken);
			if (repository.IsQueryable())
				return Task.FromResult(repository.RequireQueryable().AsQueryable().LongCount());

			throw new NotSupportedException("The repository does not support querying");
		}

        public static long Count<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter)
            where TEntity : class
            => repository.CountAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

		public static long CountAll<TEntity>(this IRepository<TEntity> repository)
			where TEntity : class
			=> repository.CountAllAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region FindById

        public static TEntity? FindById<TEntity>(this IRepository<TEntity> store, string id)
            where TEntity : class
            => store.FindByIdAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region FindFirst

        public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().FindAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

        public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
            where TEntity : class
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

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
			where TEntity : class
			=> repository.RequireFilterable().FindAllAsync(filter);

        public static IList<TEntity> FindAll<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class
            => repository.RequireFilterable().FindAllAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static IList<TEntity> FindAll<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class
            => repository.FindAll(QueryFilter.Empty);

        #endregion

        #region States

        public static void AddState<TEntity, TStatus>(this IStateRepository<TEntity, TStatus> repository, TEntity entity, EntityStateInfo<TStatus> stateInfo)
            where TEntity : class
            => repository.AddStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        public static void RemoveState<TEntity, TStatus>(this IStateRepository<TEntity, TStatus> repository, TEntity entity, EntityStateInfo<TStatus> stateInfo)
            where TEntity : class
            => repository.RemoveStateAsync(entity, stateInfo).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion
    }
}