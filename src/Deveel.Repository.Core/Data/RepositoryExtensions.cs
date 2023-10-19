// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

		/// <summary>
		/// Gets a version of the repository that is queryable.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to query
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository to get the queryable version.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="IQueryableRepository{TEntity}"/>
		/// if the repository is queryable, otherwise it throws an exception.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository is not queryable
		/// </exception>
		public static IQueryableRepository<TEntity> AsQueryable<TEntity>(this IRepository<TEntity> repository)
			where TEntity : class {
			if (!(repository is IQueryableRepository<TEntity> queryable))
				throw new NotSupportedException("The repository is not queryable");

			return queryable;
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
		public static void Add<TEntity>(this IRepository<TEntity> repository, TEntity entity)
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
		/// <param name="key">
		/// The key that uniquely identifies the entity to remove
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the entity was removed successfully,
		/// otherwise it returns <c>false</c>.
		/// </returns>
        public static async Task<bool> RemoveByKeyAsync<TEntity>(this IRepository<TEntity> repository, object key, CancellationToken cancellationToken = default)
            where TEntity : class {
            var entity = await repository.FindByKeyAsync(key, cancellationToken);
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
		/// <param name="key">
		/// The key that uniquely identifies the entity to remove.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the entity was removed successfully,
		/// otherwise it returns <c>false</c>.
		/// </returns>
		/// <seealso cref="IRepository{TEntity}.RemoveAsync(TEntity, CancellationToken)"/>
        public static bool RemoveByKey<TEntity>(this IRepository<TEntity> repository, object key)
            where TEntity : class
            => repository.RemoveByKeyAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();

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
		/// native method <see cref="IPageableRepository{TEntity}.GetPageAsync(PageQuery{TEntity}, CancellationToken)"/>.
		/// </para>
		/// <para>
		/// If the repository does not implement the interface, the method
		/// attempts to cast it to a <see cref="IQueryableRepository{TEntity}"/>
		/// and invoke the a paging operation on the <see cref="IQueryable{T}"/>.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="PageResult{TEntity}"/> that
		/// represents the result of the query.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support paging.
		/// </exception>
		public static Task<PageResult<TEntity>> GetPageAsync<TEntity>(this IRepository<TEntity> repository, PageQuery<TEntity> request, CancellationToken cancellationToken = default)
			where TEntity : class {
			if (repository.IsPageable())
				return repository.RequirePageable().GetPageAsync(request, cancellationToken);
			if (repository.IsQueryable())
				return Task.FromResult(repository.RequireQueryable().GetPage(request));

			throw new NotSupportedException("The repository does not support paging");
		}

		/// <summary>
		/// Gets a page of entities from the repository, given
		/// the page number and the size
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository to use to retrieve the page.
		/// </param>
		/// <param name="page">
		/// The number of the page, starting from 1, to retrieve from the repository.
		/// </param>
		/// <param name="size">
		/// The size of the page to retrieve from the repository.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="PageResult{TEntity}"/> that
		/// represents the result of the query.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support paging.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when the given page number is less than 1, or the given
		/// size is less than zero.
		/// </exception>
		public static Task<PageResult<TEntity>> GetPageAsync<TEntity>(this IRepository<TEntity> repository, int page, int size, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.GetPageAsync(new PageQuery<TEntity>(page, size), cancellationToken);

        /// <summary>
        /// Gets a page of entities from the repository, given
        /// the query object that defines the scope
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of entity handled by the repository.
        /// </typeparam>
        /// <param name="repository">
        /// The instance of the repository to use to retrieve the page.
        /// </param>
        /// <param name="query">
        /// The query object that defines the scope of the page to retrieve
        /// </param>
		/// <remarks>
		/// <para>
		/// This method attempts to cast the given repository to a
		/// <see cref="IPageableRepository{TEntity}"/> and invoke the
		/// native method <see cref="IPageableRepository{TEntity}.GetPageAsync(PageQuery{TEntity}, CancellationToken)"/>.
		/// </para>
		/// <para>
		/// If the repository does not implement the interface, the method
		/// attempts to cast it to a <see cref="IQueryableRepository{TEntity}"/>
		/// and invoke the a paging operation on the <see cref="IQueryable{T}"/>.
		/// </para>
		/// </remarks>
        /// <returns>
        /// Returns an instance of <see cref="PageResult{TEntity}"/> that
        /// represents the paged result of the query.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the repository does not support paging or querying.
        /// </exception>
		public static PageResult<TEntity> GetPage<TEntity>(this IRepository<TEntity> repository, PageQuery<TEntity> query)
			where TEntity : class {
			if (repository.IsPageable())
				return repository.RequirePageable().GetPage(query);
			if (repository.IsQueryable())
				return repository.RequireQueryable().GetPage(query);

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
            => repository.ExistsAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);

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
		/// The filter used to check the existence of any matching entity.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
        /// <remarks>
        /// <para>
        /// This method attempts to cast the given repository to a
        /// <see cref="IFilterableRepository{TEntity}"/> to invoke the
        /// native method <see cref="IFilterableRepository{TEntity}.ExistsAsync(IQueryFilter, CancellationToken)"/>.
        /// </para>
        /// <para>
        /// When the repository does not implement the interface, the method
        /// attempts to cast it to a <see cref="IQueryableRepository{TEntity}"/>
        /// to invoke the a query operation on the <see cref="IQueryable{T}"/>.
        /// </para>
        /// </remarks>
		/// <returns>
		/// Returns <c>true</c> if any entity exists in the repository,
		/// or <c>false</c> if not.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support querying or filtering.
		/// </exception>
		public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class {
			if (repository.IsFilterable())
				return repository.RequireFilterable().ExistsAsync(filter, cancellationToken);
			if (repository.IsQueryable())
				return Task.FromResult(repository.RequireQueryable().AsQueryable().Any(filter.AsLambda<TEntity>()));

			throw new NotSupportedException("The repository does not support querying");
		}

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
			where TEntity : class
			=> repository.ExistsAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

		/// <summary>
		/// Synchronously checks if an entity exists in the repository,
		/// given a filter expression
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository to use to check the existence
		/// of any entity that matches the given filter.
		/// </param>
		/// <param name="filter">
		/// The filter expression used to check the existence of any matching entity.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if any entity exists in the repository,
		/// otherwise it returns <c>false</c>.
		/// </returns>
		/// <seealso cref="IFilterableRepository{TEntity}.ExistsAsync(IQueryFilter, CancellationToken)"/>
        public static bool Exists<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter)
            where TEntity : class
            => Exists(repository, new ExpressionQueryFilter<TEntity>(filter));

        #endregion

        #region Count

		/// <summary>
		/// Counts the number of entities in the repository,
		/// given a filter expression
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository to use to count the entities.
		/// </param>
		/// <param name="filter">
		/// A filter expression used to count the matching entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns the number of entities in the repository that match
		/// the given filter.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support querying or filtering.
		/// </exception>
        public static Task<long> CountAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class {
			if (repository.IsFilterable())
				return repository.RequireFilterable().CountAsync(new ExpressionQueryFilter<TEntity>(filter), cancellationToken);
			if (repository.IsQueryable())
				return Task.FromResult(repository.RequireQueryable().AsQueryable().LongCount(filter));

			throw new NotSupportedException("The repository does not support querying");
		}

		/// <summary>
		/// Counts the number of entities in the repository
		/// </summary>
		/// <typeparam name="TEntity">
        /// The type of entity handled by the repository.
        /// </typeparam>
		/// <param name="repository">
        /// The instance of the repository to use to count the entities.
        /// </param>
		/// <param name="cancellationToken">
        /// A token used to cancel the operation.
        /// </param>
		/// <returns>
        /// Returns the number of entities in the repository.
        /// </returns>
		/// <exception cref="NotSupportedException">
        /// Thrown when the repository does not support querying or filtering.
        /// </exception>
        /// <seealso cref="IFilterableRepository{TEntity}.CountAsync(IQueryFilter, CancellationToken)"/>
		public static Task<long> CountAllAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
			where TEntity : class {
			if (repository.IsFilterable())
				return repository.RequireFilterable().CountAsync(QueryFilter.Empty, cancellationToken);
			if (repository.IsQueryable())
				return Task.FromResult(repository.RequireQueryable().AsQueryable().LongCount());

			throw new NotSupportedException("The repository does not support querying");
		}

        /// <summary>
        /// Counts the number of entities in the repository,
        /// given a filter expression to match.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of entity handled by the repository.
        /// </typeparam>
        /// <param name="repository">
        /// The instance of the repository to use to count the entities.
        /// </param>
        /// <param name="filter">
        /// The filter used to count the matching entities.
        /// </param>
        /// <returns>
        /// Returns the number of entities in the repository that match
        /// the given filter.
        /// </returns>
        /// <seealso cref="CountAsync{TEntity}(IRepository{TEntity}, Expression{Func{TEntity, bool}}, CancellationToken)"/>
        public static long Count<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter)
            where TEntity : class
            => repository.CountAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Counts the number of entities in the repository
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of entity handled by the repository.
        /// </typeparam>
        /// <param name="repository">
        /// The instance of the repository to use to count the entities.
        /// </param>
        /// <returns>
        /// Returns the number of entities in the repository.
        /// </returns>
        /// <seealso cref="CountAllAsync{TEntity}(IRepository{TEntity}, CancellationToken)"/>
		public static long CountAll<TEntity>(this IRepository<TEntity> repository)
			where TEntity : class
			=> repository.CountAllAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region FindById

        /// <summary>
        /// Finds a single entity in the repository, given the key
        /// that uniquely identifies it
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of entity handled by the repository.
        /// </typeparam>
        /// <param name="repository">
        /// The instance of the repository to use to find the entity.
        /// </param>
        /// <param name="key">
        /// The key that uniquely identifies the entity to find.
        /// </param>
        /// <returns>
        /// Returns an instance of <typeparamref name="TEntity"/> that
        /// is identified by the given key, or <c>null</c> if no entity
        /// with the given key exists in the repository.
        /// </returns>
        /// <seealso cref="IRepository{TEntity}.FindByKeyAsync(object, CancellationToken)"/>
        public static TEntity? FindByKey<TEntity>(this IRepository<TEntity> repository, object key)
            where TEntity : class
            => repository.FindByKeyAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();

        #endregion

        #region FindFirst

		/// <summary>
		/// Finds the first entity in the repository that matches
		/// the given filter expression
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The repository instance to use to find the entity.
		/// </param>
		/// <param name="filter">
		/// The filter expression used to find the entity.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TEntity"/> that
		/// can be identified by the given filter, or <c>null</c> if no
		/// entity matches the given filter.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support filtering.
		/// </exception>
        public static Task<TEntity?> FindFirstAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().FindAsync(Query.Where(new ExpressionQueryFilter<TEntity>(filter)), cancellationToken);

		/// <summary>
		/// Finds the first entity in the repository, naturally ordered.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The repository instance to use to find the entity.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TEntity"/> that
		/// is the first entity in the repository, or <c>null</c> if the
		/// repository is empty.
		/// </returns>
        public static Task<TEntity?> FindFirstAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().FindAsync(Query.Empty, cancellationToken);

		/// <summary>
		/// Finds the first entity in the repository that matches
		/// the given filter expression
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The repository instance to use to find the entity.
		/// </param>
		/// <param name="filter">
		/// The filter expression used to find the entity.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TEntity"/> that
		/// can be identified by the given filter, or <c>null</c> if no
		/// entity matches the given filter.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support filtering.
		/// </exception>
		public static TEntity? FindFirst<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class
            => repository.RequireFilterable().FindAsync(Query.Where(filter)).ConfigureAwait(false).GetAwaiter().GetResult();

		/// <summary>
		/// Finds the first entity in the repository, naturally ordered.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The repository instance to use to find the entity.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TEntity"/> that
		/// is the first entity in the repository, or <c>null</c> if the
		/// repository is empty.
		/// </returns>
		public static TEntity? FindFirst<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class
            => repository.RequireFilterable().FindFirst(QueryFilter.Empty);

        #endregion


        #region FindAll

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().FindAllAsync(Query.Where(new ExpressionQueryFilter<TEntity>(filter)), cancellationToken);

        public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, CancellationToken cancellationToken = default)
            where TEntity : class
            => repository.RequireFilterable().FindAllAsync(Query.Empty, cancellationToken);

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
			where TEntity : class
			=> repository.RequireFilterable().FindAllAsync(Query.Where(filter));

        public static IList<TEntity> FindAll<TEntity>(this IRepository<TEntity> repository, IQueryFilter filter)
            where TEntity : class
            => repository.RequireFilterable().FindAllAsync(filter).ConfigureAwait(false).GetAwaiter().GetResult();

        public static IList<TEntity> FindAll<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class
            => repository.FindAll(QueryFilter.Empty);

        #endregion
    }
}