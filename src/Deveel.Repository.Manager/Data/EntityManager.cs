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

using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Data {
	/// <summary>
	/// A service that provides a set of operations to manage a specific
	/// entities in a repository.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity managed by the service.
	/// </typeparam>
	public class EntityManager<TEntity> : IDisposable, IAsyncDisposable where TEntity : class {
		private bool disposedValue;

		/// <summary>
		/// Constructs the service with the given repository.
		/// </summary>
		/// <param name="repository">
		/// The repository that is providing the data access to the entities.
		/// </param>
		/// <param name="validator">
		/// An optional service used to validate the entity before it is added
		/// or updated in the repository.
		/// </param>
		/// <param name="services">
		/// The services used to resolve the dependencies of the manager.
		/// </param>
		/// <param name="loggerFactory">
		/// A factory used to create a logger for the manager.
		/// </param>
		public EntityManager(
			IRepository<TEntity> repository, 
			IEntityValidator<TEntity>? validator = null,
			IServiceProvider? services = null, 
			ILoggerFactory? loggerFactory = null) {
			ArgumentNullException.ThrowIfNull(repository, nameof(repository));

			Repository = repository;
			EntityValidator = validator;
			Services = services;
			Logger = loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
		}

		/// <summary>
		/// Destructs the instance of the service.
		/// </summary>
		~EntityManager() {
			Dispose(disposing: false);
		}

		/// <summary>
		/// Gets the instance of the service provider used to resolve
		/// the dependencies of the service.
		/// </summary>
		protected IServiceProvider? Services { get; private set; }

		/// <summary>
		/// Gets the instance of the factory used to create errors
		/// </summary>
		protected IOperationErrorFactory? ErrorFactory => Services?.GetService<IOperationErrorFactory>();

		/// <summary>
		/// Gets the service used to validate the entity before
		/// it is added or updated in the repository.
		/// </summary>
		protected IEntityValidator<TEntity>? EntityValidator { get; }

		/// <summary>
		/// Gets the logger used to log messages from the service.
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// Gets the repository that is providing the data access
		/// to the entities.
		/// </summary>
		protected IRepository<TEntity> Repository { get; private set; }

		/// <summary>
		/// Gets the cancellation token used to cancel operations
		/// </summary>
		protected virtual CancellationToken CancellationToken 
			=> Services?.GetService<IOperationCancellationSource>()?.Token ?? default;

        /// <summary>
        /// Gets a value indicating if the repository is multi-tenant
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Throws when the service has been disposed.
        /// </exception>
        public virtual bool IsMultiTenant {
			get {
				ThrowIfDisposed();
				return (Repository is IMultiTenantRepository<TEntity>);
			}
		}

        /// <summary>
        /// When the repository is multi-tenant, gets the identifier
        /// of the tenant that is being managed by the service.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Throws when the service has been disposed.
        /// </exception>
        protected virtual string? TenantId {
			get {
				ThrowIfDisposed();

				if (Repository is IMultiTenantRepository<TEntity> multiTenant)
					return multiTenant.TenantId;

				return null;
			}
		}

        /// <summary>
        /// Gets a value indicating if the repository supports paging
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Throws when the service has been disposed.
        /// </exception>
        public virtual bool SupportsPaging {
			get {
				ThrowIfDisposed();

				return (Repository is IPageableRepository<TEntity>);
			}
		}

        /// <summary>
        /// Gets the repository that supports paging
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown when the repository does not support paging
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Throws when the service has been disposed.
        /// </exception>
		protected virtual IPageableRepository<TEntity> PageableRepository {
			get {
				ThrowIfDisposed();

				if (!(Repository is IPageableRepository<TEntity> pageable))
					throw new NotSupportedException("The repository does not support paging");

				return pageable;
			}
		}

        /// <summary>
        /// Gets a value indicating if the repository supports queries
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Throws when the service has been disposed.
        /// </exception>
        public virtual bool SupportsQueries {
			get {
				ThrowIfDisposed();

				return (Repository is IQueryableRepository<TEntity>);
			}
		}

        /// <summary>
        /// When the repository supports queries, gets the queryable
        /// instance of the repository.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Throws when the service has been disposed.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when the repository does not support queries
        /// </exception>
        protected virtual IQueryable<TEntity> Entities {
			get {
				ThrowIfDisposed();

				if (!(Repository is IQueryableRepository<TEntity> queryable))
					throw new NotSupportedException("The repository does not support queries");

				return queryable.AsQueryable();
			}
		}

        /// <summary>
        /// Gets a value indicating if the repository supports filters
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the service has been disposed.
        /// </exception>
		public virtual bool SupportsFilters {
			get {
				ThrowIfDisposed();

				return (Repository is IFilterableRepository<TEntity>);
			}
		}

        /// <summary>
        /// Gets the repository that supports filters
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown when the repository does not support filters
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when the service has been disposed.
        /// </exception>
		protected virtual IFilterableRepository<TEntity> FilterableRepository {
			get {
				ThrowIfDisposed();

				if (!(Repository is IFilterableRepository<TEntity> filterable))
					throw new NotSupportedException("The repository does not support filters");

				return filterable;
			}
		}

        // shortcut to the logger method
        private void LogUnknownError(Exception ex) {
            Logger.LogUnknownError(typeof(TEntity), ex);
        }

        private void LogEntityUnknownError(object? entityKey, Exception ex) {
            Logger.LogEntityUnknownError(typeof(TEntity), entityKey, ex);
        }


        /// <summary>
        /// Checks if the service has been disposed and
        /// eventually throws an exception.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Throws when the service has been disposed.
        /// </exception>
        protected void ThrowIfDisposed() {
			if (disposedValue)
				throw new ObjectDisposedException(GetType().Name);
		}

		/// <summary>
		/// Actively disposes the service.
		/// </summary>
		/// <param name="disposing">
		/// Indicates whether the service is being disposed.
		/// </param>
		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					// TODO: dispose managed state (managed objects)
				}

				Repository = null;
				Services = null;
				disposedValue = true;
			}
		}

		/// <inheritdoc/>
		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public ValueTask DisposeAsync() {
			Dispose(disposing: false);
			GC.SuppressFinalize(this);

			return default;
		}

		/// <summary>
		/// Creates an error object with the given code and message.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of error.
		/// </param>
		/// <param name="message">
		/// An optional message that describes the error.
		/// </param>
		/// <remarks>
		/// The default implementation of this method uses the
		/// <see cref="ErrorFactory"/> to create the error object,
		/// if any is available, otherwise it creates a new instance
		/// of <see cref="Deveel.OperationError"/>.
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="IOperationError"/> that
		/// describes the error.
		/// </returns>
		protected virtual IOperationError OperationError(string errorCode, string? message = null)
			=> ErrorFactory?.CreateError(errorCode, message) ?? new OperationError(errorCode, message);

        /// <summary>
        /// Creates a validation error object with the given 
        /// code and validation results.
        /// </summary>
        /// <param name="errorCode">
        /// The code that identifies the class of validation error.
        /// </param>
        /// <param name="validationResults">
        /// The list of validation results that describe the error.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="IValidationError"/> that
        /// represents a validation error.
        /// </returns>
		protected virtual IValidationError ValidationError(string errorCode, IList<ValidationResult> validationResults)
			=> ErrorFactory?.CreateValidationError(errorCode, validationResults) ?? new EntityValidationError(errorCode, validationResults);

        /// <summary>
        /// Creates an error object with the given code and message
        /// </summary>
        /// <param name="errorCode">
        /// The code that identifies the class of error.
        /// </param>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="OperationResult"/> that
        /// represents a failed operation.
        /// </returns>
        /// <see cref="OperationError(string, string?)"/>
        protected OperationResult Fail(string errorCode, string? message = null)
			=> OperationResult.Fail(OperationError(errorCode, message));

		/// <summary>
		/// Creates an error object from the given operation exception.
		/// </summary>
		/// <param name="error">
		/// The exception that caused the error.
		/// </param>
        /// <remarks>
        /// The default implementation of this method uses the
        /// information from the given exception to create an error
        /// invoking the <see cref="Fail(string, string?)"/> method.
        /// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents a failed operation.
		/// </returns>
        /// <seealso cref="Fail(string, string?)"/>
		protected OperationResult Fail(OperationException error)
			=> Fail(error.ErrorCode, error.Message);

		/// <summary>
		/// Creates an error object from a validation error.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of validation error.
		/// </param>
		/// <param name="validationResults">
		/// The list of validation results that describe the error.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents a failed validation of an entity.
		/// </returns>
        /// <see cref="ValidationError(string, IList{ValidationResult})"/>
		protected OperationResult ValidationFailed(string errorCode, IList<ValidationResult> validationResults)
			=> OperationResult.Fail(ValidationError(errorCode, validationResults));

		/// <summary>
		/// Creates a result object that represents a successful operation.
		/// </summary>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents a successful operation.
		/// </returns>
		protected OperationResult Success() => OperationResult.Success;

		/// <summary>
		/// Creates a result object that represents an operation that
		/// has not modified the state of the entity.
		/// </summary>
		/// <returns>
        /// Returns an instance of <see cref="OperationResult"/> that
        /// represents an operation that has not modified the state of
        /// an entity.
        /// </returns>
		protected OperationResult NotModified() => OperationResult.NotModified;

        /// <summary>
        /// Validates the given entity before it is added or updated
        /// </summary>
        /// <param name="entity">
        /// The entity to be validated.
        /// </param>
        /// <returns>
        /// Returns a list of <see cref="ValidationResult"/> that
        /// describe the validation errors.
        /// </returns>
		protected virtual async Task<IList<ValidationResult>> ValidateAsync(TEntity entity) {
			if (EntityValidator == null)
				return new List<ValidationResult>();

			var results = new List<ValidationResult>();

			await foreach(var result in EntityValidator.ValidateAsync(this, entity, CancellationToken)) {
				if (result != null)
					results.Add(result);
			}

			return results;
		}

        /// <summary>
        /// Gets the key of the given entity
        /// </summary>
        /// <param name="entity">
        /// The entity to get the key from.
        /// </param>
        /// <remarks>
        /// The default implementation of this method uses the
        /// <see cref="IRepository{TEntity}.GetEntityKey(TEntity)"/>
        /// method to get the key of the entity.
        /// </remarks>
        /// <returns>
        /// Returns an object that represents the key of the entity,
        /// or <c>null</c> if the entity does not have a valid key.
        /// </returns>
		protected virtual object? GetEntityKey(TEntity entity) {
			return Repository.GetEntityKey(entity);
		}

        /// <summary>
        /// A callback invoked when an entity is being added 
        /// to the repository.
        /// </summary>
        /// <param name="entity">
        /// The entity that is being added.
        /// </param>
        /// <remarks>
        /// When <see cref="AddRangeAsync(IEnumerable{TEntity})"/> is
        /// invoked, this method is invoked for each entity in the
        /// range of entities.
        /// </remarks>
        /// <returns>
        /// Returns the entity that is being added, after
        /// any modification done by the callback.
        /// </returns>
		protected virtual Task<TEntity> OnAddingEntityAsync(TEntity entity) {
			return Task.FromResult(entity);
		}

        /// <summary>
        /// Adds the given entity to the repository.
        /// </summary>
        /// <param name="entity">
        /// The entity to be added.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="OperationResult"/> that
        /// describes the result of the operation.
        /// </returns>
		public virtual async Task<OperationResult> AddAsync(TEntity entity) {
			ThrowIfDisposed();

			try {
				Logger.LogAddingEntity();

				var validation = await ValidateAsync(entity);
				if (validation != null && validation.Count > 0) {
					Logger.LogEntityNotValid(typeof(TEntity));
					return ValidationFailed(EntityErrorCodes.NotValid, validation);
				}

				entity = await OnAddingEntityAsync(entity);

				await Repository.AddAsync(entity, CancellationToken);

				Logger.LogEntityAdded(GetEntityKey(entity)!);

				return Success();
			} catch(Exception ex) {
				LogUnknownError(ex);
				return Fail(EntityErrorCodes.UnknownError, "An unknown error occurred while adding the entity");
			}
		}

        /// <summary>
        /// Adds the given range of entities to the repository.
        /// </summary>
        /// <param name="entities">
        /// The range of entities to be added.
        /// </param>
        /// <remarks>
        /// The default implementation of this method attempts to
        /// validate each entity in the range before adding it to
        /// the repository, and if any validation error is found,
        /// the method returns a failure result.
        /// </remarks>
        /// <returns>
        /// Returns a result object that describes the result of the
        /// operation.
        /// </returns>
        /// <seealso cref="IRepository{TEntity}.AddRangeAsync(IEnumerable{TEntity}, CancellationToken)"/>
		public virtual async Task<OperationResult> AddRangeAsync(IEnumerable<TEntity> entities) {
			try {
				Logger.LogAddingEntityRange();

				var toBeAdded = new List<TEntity>();
				foreach (var entity in entities) {
					var item = await OnAddingEntityAsync(entity);

					var validation = await ValidateAsync(item);
					if (validation != null && validation.Count > 0)
						return ValidationFailed(EntityErrorCodes.NotValid, validation);

					toBeAdded.Add(item);
				}

				await Repository.AddRangeAsync(toBeAdded, CancellationToken);

                Logger.LogEntityRangeAdded();

				return Success();
			} catch (Exception ex) {
				LogUnknownError(ex);
				return Fail(EntityErrorCodes.UnknownError, "An unknown error occurred while adding the entities");
			}
		}

        /// <summary>
        /// A callback invoked when an entity is being updated
        /// </summary>
        /// <param name="entity">
        /// The entity that is being updated.
        /// </param>
        /// <returns>
        /// Returns the entity that is being updated, after
        /// the callback has modified it.
        /// </returns>
		protected virtual Task<TEntity> OnUpdatingEntityAsync(TEntity entity) {
			return Task.FromResult(entity);
		}

        /// <summary>
        /// Checks if the given entities are equal.
        /// </summary>
        /// <param name="existing">
        /// The entity already in the repository.
        /// </param>
        /// <param name="other">
        /// The entity provided by the caller for update.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method is used by the update operation to check
        /// if the entity in the repository is the same as the one
        /// provided by the caller, and eventually skip the update
        /// on the repository, to ensure idempotency.
        /// </para>
        /// <para>
        /// The default implementation of this method tries to
        /// use the <see cref="IEquatable{T}.Equals(T)"/> method
        /// to check if the entities are equal, otherwise it uses
        /// the native <see cref="object.Equals(object)"/> method.
        /// </para>
        /// </remarks>
        /// <returns>
        /// Returns <c>true</c> if the entities are equal, otherwise
        /// it returns <c>false</c>.
        /// </returns>
        protected virtual bool AreEqual(TEntity existing, TEntity other) {
            if (existing == null && other == null)
                return true;
            if (existing == null)
                return false;

			if (typeof(IEquatable<TEntity>).IsAssignableFrom(typeof(TEntity)))
				return ((IEquatable<TEntity>)existing).Equals(other);

			return existing.Equals(other);
		}

        /// <summary>
        /// Updates the given entity in the repository.
        /// </summary>
        /// <param name="entity">
        /// The entity to be updated.
        /// </param>
        /// <remarks>
        /// <para>
        /// The default implementation of this method first
        /// attempts to find an entity in the repository with
        /// the same key of the given entity, and if found, it
        /// verifies the equality of the two entities using the
        /// <see cref="AreEqual(TEntity, TEntity)"/> function.
        /// If the two entities are equal, the method returns
        /// quickly an instance of <see cref="OperationResult"/>
        /// that indicates the entity was not modified.
        /// </para>
        /// <para>
        /// The method can reurn a failure result if the entity 
        /// does not have an associated key, or if the entity is 
        /// not found in the repository.
        /// </para>
        /// </remarks>
        /// <returns>
        /// Returns a result object that describes the result of the
        /// update operation.
        /// </returns>
		public virtual async Task<OperationResult> UpdateAsync(TEntity entity) {
			ThrowIfDisposed();

			var entityKey = GetEntityKey(entity);

			try {
				if (entityKey == null)
					return Fail(EntityErrorCodes.NotValid, "The entity does not have a valid key");

				Logger.LogUpdatingEntity(entityKey);

				var existing = await FindByKeyAsync(entityKey);
				if (existing == null)
					return Fail(EntityErrorCodes.NotFound, "The entity was not found in the repository");

				if (AreEqual(existing, entity))
					return NotModified();

				var validation = await ValidateAsync(entity);
				if (validation != null && validation.Count > 0) {
					Logger.LogEntityNotValid(typeof(TEntity));
					return ValidationFailed(EntityErrorCodes.NotValid, validation);
				}

				entity = await OnUpdatingEntityAsync(entity);

				if (!await Repository.UpdateAsync(entity, CancellationToken)) {
					Logger.LogEntityNotModified(typeof(TEntity), entityKey);
					return NotModified();
				}

				Logger.LogEntityUpdated(entityKey);

				return Success();
			} catch (Exception ex) {
				LogEntityUnknownError(entityKey, ex);
				return Fail(EntityErrorCodes.UnknownError, "An unknown error occurred while updating the entity");
			}
		}

        /// <summary>
        /// Removes the given entity from the repository.
        /// </summary>
        /// <param name="entity">
        /// The entity to be removed.
        /// </param>
        /// <remarks>
        /// <para>
        /// The default implementation of this method first tries
        /// to find an entity in the repository with the same key
        /// and if not found, it returns a failure result.
        /// </para>
        /// <para>
        /// If the entity is found, the method attempts to remove
        /// it from the repository, and if the operation is not
        /// performed by the repository, it returns a result that
        /// indicates the entity was not modified.
        /// </para>
        /// </remarks>
        /// <returns>
        /// Returns an instance of <see cref="OperationResult"/> that
        /// describes the result of the operation.
        /// </returns>
		public virtual async Task<OperationResult> RemoveAsync(TEntity entity) {
			ThrowIfDisposed();

			var entityKey = GetEntityKey(entity);

			try {
				if (entityKey == null)
					return Fail(EntityErrorCodes.NotValid, "The entity does not have a valid key");

				Logger.LogRemovingEntity(entityKey);

				var found = await FindByKeyAsync(entityKey);
				if (found == null)
					return Fail(EntityErrorCodes.NotFound, "The entity was not found in the repository");

				if (!await Repository.RemoveAsync(found, CancellationToken)) {
					Logger.LogEntityNotRemoved(entityKey);
					return NotModified();
				}

				return Success();
			} catch (Exception ex) {
				LogEntityUnknownError(entityKey, ex);
				return Fail(EntityErrorCodes.UnknownError, "An unknown error occurred while removing the entity");
			}
		}

        /// <summary>
        /// Removes the given range of entities from the repository.
        /// </summary>
        /// <param name="entities">
        /// The range of entities to be removed.
        /// </param>
        /// <returns>
        /// Returns an instance of <see cref="OperationResult"/> that
        /// describes the result of the operation.
        /// </returns>
		public virtual async Task<OperationResult> RemoveRangeAsync(IEnumerable<TEntity> entities) {
			ThrowIfDisposed();

			try {
                Logger.LogRemovingEntityRange();

				// TODO: should we check for the entities to be valid?

				await Repository.RemoveRangeAsync(entities, CancellationToken);

                Logger.LogEntityRangeRemoved();

				return Success();
			} catch (Exception ex) {
				LogUnknownError(ex);
				return Fail(EntityErrorCodes.UnknownError, "An unknown error occurred while removing the entities");
			}
		}

        /// <summary>
        /// Finds an entity in the repository with the given key.
        /// </summary>
        /// <param name="key">
        /// The key of the entity to be found.
        /// </param>
        /// <returns>
        /// Returns an instance of <typeparamref name="TEntity"/> that
        /// is identified by the given key, or <c>null</c> if no entity
        /// was found for the given key.
        /// </returns>
        /// <exception cref="OperationException">
        /// Thrown when an unknown error occurs while looking for the entity.
        /// </exception>
        // TODO: Is there any use case for using OperationResult<TEntity> here
        //       instead of returning an entity?
        public virtual async Task<TEntity?> FindByKeyAsync(object key) {
			ThrowIfDisposed();

			ArgumentNullException.ThrowIfNull(key, nameof(key));

			try {
				// TODO: log this operation

				return await Repository.FindByKeyAsync(key, CancellationToken);
			} catch (Exception ex) {
				LogEntityUnknownError(key, ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

        /// <summary>
        /// Finds an entity in the repository that matches the given filter.
        /// </summary>
        /// <param name="filter">
        /// The filter to be used to look for the entity.
        /// </param>
        /// <returns>
        /// Returns the first instance of <typeparamref name="TEntity"/> that
        /// matches the given filter, or <c>null</c> if no entity was found.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the repository does not support filters.
        /// </exception>
        /// <exception cref="OperationException">
        /// Thrown when an unknown error occurs while looking for the entity.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the given filter is <c>null</c>.
        /// </exception>
        /// <seealso cref="IFilterableRepository{TEntity}.FindAsync(IQueryFilter, CancellationToken)"/>
        // TODO: Is there any use case for using OperationResult<TEntity> here
        //       instead of returning the entity?
        public virtual async Task<TEntity?> FindFirstAsync(IQueryFilter filter) {
			ThrowIfDisposed();

			if (!SupportsFilters)
				throw new NotSupportedException("The repository does not support filters");

            ArgumentNullException.ThrowIfNull(filter, nameof(filter));

			try {
				return await FilterableRepository.FindAsync(filter, CancellationToken);
			} catch (Exception ex) {
				LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

        /// <summary>
        /// Finds the first entity in the repository that matches the 
        /// given filter expression.
        /// </summary>
        /// <param name="filter">
        /// The filter expression to be used to look for the entity.
        /// </param>
        /// <returns>
        /// Returns the first instance of <typeparamref name="TEntity"/> that
        /// mathces the given filter, or <c>null</c> if no entity was found.
        /// </returns>
        /// <seealso cref="FindFirstAsync(IQueryFilter)"/>
		public Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>>? filter = null)
            => FindFirstAsync(filter == null ? QueryFilter.Empty : QueryFilter.Where(filter));

        /// <summary>
        /// Finds all the entities in the repository that match the given filter.
        /// </summary>
        /// <param name="filter">
        /// The filter to be used to look for the entities.
        /// </param>
        /// <returns>
        /// Returns a list of <typeparamref name="TEntity"/> that match the
        /// given filter.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the repository does not support filters.
        /// </exception>
        /// <exception cref="OperationException">
        /// Thrown when an unknown error occurs while looking for the entities.
        /// </exception>
        // TODO: Is there any use case for using OperationResult<IList<TEntity>> here
        //       instead of returning a list of entities?
		public virtual async Task<IList<TEntity>> FindAllAsync(IQueryFilter filter) {
			ThrowIfDisposed();

			if (!SupportsFilters)
				throw new NotSupportedException("The repository does not support filters");

			try {
                // TODO: log this operation ...

				return await FilterableRepository.FindAllAsync(filter, CancellationToken);
			} catch (Exception ex) {
				LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

        /// <summary>
        /// Finds all the entities in the repository that match the given 
        /// filter expression.
        /// </summary>
        /// <param name="filter">
        /// The filter expression to be used to look for the entities.
        /// </param>
        /// <remarks>
        /// This method is a shortcut to the <see cref="FindAllAsync(IQueryFilter)"/>
        /// using an instance of <see cref="ExpressionQueryFilter{TEntity}"/> as
        /// argument.
        /// </remarks>
        /// <returns>
        /// Returns a list of <typeparamref name="TEntity"/> that match the
        /// given filter.
        /// </returns>
        /// <seealso cref="FindAllAsync(IQueryFilter)"/>
        /// <seealso cref="ExpressionQueryFilter{TEntity}"/>
        /// <exception cref="NotSupportedException">
        /// Thrown when the repository does not support filters.
        /// </exception>
        public Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>>? filter = null)
            => FindAllAsync(filter == null ? QueryFilter.Empty : QueryFilter.Where(filter));

        /// <summary>
        /// Counts the number of entities in the repository that match
        /// the given filter.
        /// </summary>
        /// <param name="filter">
        /// The filter to be used to look for the entities.
        /// </param>
        /// <returns>
        /// Returns the number of entities that match the given filter.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the repository does not support filters.
        /// </exception>
        /// <exception cref="OperationException">
        /// Thrown when an unknown error occurs while looking for the entities.
        /// </exception>
		public virtual Task<long> CountAsync(IQueryFilter filter) {
			ThrowIfDisposed();

			if (!SupportsFilters)
				throw new NotSupportedException("The repository does not support filters");

			ArgumentNullException.ThrowIfNull(filter, nameof(filter));

			try {
				return FilterableRepository.CountAsync(filter, CancellationToken);
			} catch (Exception ex) {
				LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

        /// <summary>
        /// Counts the number of entities in the repository that match
        /// the given filter expression.
        /// </summary>
        /// <param name="filter">
        /// The filter expression to be used to look for the entities.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method is a shortcut to the <see cref="CountAsync(IQueryFilter)"/>
        /// overload, using a <see cref="ExpressionQueryFilter{TEntity}"/> as
        /// argument of the method.
        /// </para>
        /// <para>
        /// When the given filter is <c>null</c>, the method uses an
        /// instance of <see cref="QueryFilter.Empty"/> as argument,
        /// that has the effect to count all the entities in the repository.
        /// </para>
        /// </remarks>
        /// <returns>
        /// Returns the number of entities that match the given filter.
        /// </returns>
		public Task<long> CountAsync(Expression<Func<TEntity, bool>>? filter = null)
            => CountAsync(filter == null ? QueryFilter.Empty : QueryFilter.Where(filter));

		public virtual async Task<PageResult<TEntity>> GetPageAsync(PageQuery<TEntity> query) {
			ThrowIfDisposed();

			if (!SupportsPaging)
				throw new NotSupportedException("The repository does not support paging");

			try {
				// TODO: log this operation

				return await PageableRepository.GetPageAsync(query, CancellationToken);
			} catch (Exception ex) {
				LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}
	}
}
