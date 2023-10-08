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

		public virtual bool IsMultiTenant {
			get {
				ThrowIfDisposed();
				return (Repository is IMultiTenantRepository<TEntity>);
			}
		}

		protected virtual string? TenantId {
			get {
				ThrowIfDisposed();

				if (Repository is IMultiTenantRepository<TEntity> multiTenant)
					return multiTenant.TenantId;

				return null;
			}
		}

		public virtual bool SupportsPaging {
			get {
				ThrowIfDisposed();

				return (Repository is IPageableRepository<TEntity>);
			}
		}

		protected virtual IPageableRepository<TEntity> PageableRepository {
			get {
				ThrowIfDisposed();

				if (!(Repository is IPageableRepository<TEntity> pageable))
					throw new NotSupportedException("The repository does not support paging");

				return pageable;
			}
		}

		public virtual bool SupportsQueries {
			get {
				ThrowIfDisposed();

				return (Repository is IQueryableRepository<TEntity>);
			}
		}

		protected virtual IQueryable<TEntity> Entities {
			get {
				ThrowIfDisposed();

				if (!(Repository is IQueryableRepository<TEntity> queryable))
					throw new NotSupportedException("The repository does not support queries");

				return queryable.AsQueryable();
			}
		}

		public virtual bool SupportsFilters {
			get {
				ThrowIfDisposed();

				return (Repository is IFilterableRepository<TEntity>);
			}
		}

		protected virtual IFilterableRepository<TEntity> FilterableRepository {
			get {
				ThrowIfDisposed();

				if (!(Repository is IFilterableRepository<TEntity> filterable))
					throw new NotSupportedException("The repository does not support filters");

				return filterable;
			}
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
		/// of <see cref="OperationError"/>.
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="IOperationError"/> that
		/// describes the error.
		/// </returns>
		protected virtual IOperationError OperationError(string errorCode, string? message = null)
			=> ErrorFactory?.CreateError(errorCode, message) ?? new OperationError(errorCode, message);

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
		protected OperationResult Fail(string errorCode, string? message = null)
			=> OperationResult.Fail(OperationError(errorCode, message));

		/// <summary>
		/// Creates an error object from the given operation exception.
		/// </summary>
		/// <param name="error">
		/// The exception that caused the error.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents a failed operation.
		/// </returns>
		protected OperationResult Fail(OperationException error)
			=> OperationResult.Fail(error);

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
		/// <returns></returns>
		protected OperationResult NotModified() => OperationResult.NotModified;


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

		protected virtual object? GetEntityKey(TEntity entity) {
			return Repository.GetEntityKey(entity);
		}

		protected virtual Task<TEntity> OnAddingEntityAsync(TEntity entity) {
			return Task.FromResult(entity);
		}

		public virtual async Task<OperationResult> AddAsync(TEntity entity) {
			ThrowIfDisposed();

			try {
				Logger.LogAddingEntity();

				var validation = await ValidateAsync(entity);
				if (validation != null && validation.Count > 0) {
					Logger.LogEntityNotValid();
					return ValidationFailed(EntityErrorCodes.NotValid, validation);
				}

				entity = await OnAddingEntityAsync(entity);

				await Repository.AddAsync(entity, CancellationToken);

				Logger.LogEntityAdded(GetEntityKey(entity)!);

				return Success();
			} catch(Exception ex) {
				Logger.LogUnknownError(ex);
				return Fail(EntityErrorCodes.UnknownError, "An unknown error occurred while adding the entity");
			}
		}

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

				// TODO: log the entities added

				return Success();
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				return Fail(EntityErrorCodes.UnknownError, "An unknown error occurred while adding the entities");
			}
		}

		protected virtual Task<TEntity> OnUpdatingEntityAsync(TEntity entity) {
			return Task.FromResult(entity);
		}

		protected virtual bool AreEqual(TEntity entity1, TEntity entity2) {
			if (typeof(IEquatable<TEntity>).IsAssignableFrom(typeof(TEntity)))
				return ((IEquatable<TEntity>)entity1).Equals(entity2);

			return entity1.Equals(entity2);
		}

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
					Logger.LogEntityNotValid();
					return ValidationFailed(EntityErrorCodes.NotValid, validation);
				}

				entity = await OnUpdatingEntityAsync(entity);

				if (!await Repository.UpdateAsync(entity, CancellationToken)) {
					Logger.LogEntityNotModified(entityKey);
					return NotModified();
				}

				Logger.LogEntityUpdated(entityKey);

				return Success();
			} catch (Exception ex) {
				Logger.LogEntityUnknownError(entityKey, ex);
				return Fail(EntityErrorCodes.UnknownError, "An unknown error occurred while updating the entity");
			}
		}

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
				Logger.LogEntityUnknownError(entityKey, ex);
				return Fail(EntityErrorCodes.UnknownError, "An unknown error occurred while removing the entity");
			}
		}

		public virtual async Task<OperationResult> RemoveRangeAsync(IEnumerable<TEntity> entities) {
			ThrowIfDisposed();

			try {
				// TODO: log this operation
				// TODO: should we check for the entities to be valid?

				await Repository.RemoveRangeAsync(entities, CancellationToken);

				return Success();
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				return Fail(EntityErrorCodes.UnknownError, "An unknown error occurred while removing the entities");
			}
		}

		public virtual async Task<TEntity?> FindByKeyAsync(object key) {
			ThrowIfDisposed();

			ArgumentNullException.ThrowIfNull(key, nameof(key));

			try {
				// TODO: log this operation

				return await Repository.FindByKeyAsync(key, CancellationToken);
			} catch (OperationException ex) {
				Logger.LogEntityUnknownError(key, ex);
				throw;
			} catch (Exception ex) {
				Logger.LogEntityUnknownError(key, ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

		public virtual async Task<TEntity?> FindAsync(IQueryFilter filter) {
			ThrowIfDisposed();

			if (!SupportsFilters)
				throw new NotSupportedException("The repository does not support filters");

			try {
				return await FilterableRepository.FindAsync(filter, CancellationToken);
			} catch (OperationException ex) {
				Logger.LogUnknownError(ex);
				throw;
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

		public Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> filter) {
			return FindAsync(QueryFilter.Where(filter));
		}

		public virtual async Task<IList<TEntity>> FindAllAsync(IQueryFilter filter) {
			ThrowIfDisposed();

			if (!SupportsFilters)
				throw new NotSupportedException("The repository does not support filters");

			try {
				return await FilterableRepository.FindAllAsync(filter, CancellationToken);
			} catch (OperationException ex) {
				Logger.LogUnknownError(ex);
				throw;
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

		public virtual Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter) {
			return FindAllAsync(QueryFilter.Where(filter));
		}

		public virtual Task<long> CountAsync(IQueryFilter filter) {
			ThrowIfDisposed();

			if (!SupportsFilters)
				throw new NotSupportedException("The repository does not support filters");

			ArgumentNullException.ThrowIfNull(filter, nameof(filter));

			try {
				return FilterableRepository.CountAsync(filter, CancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

		public virtual Task<long> CountAsync(Expression<Func<TEntity, bool>>? filter = null) {
			return CountAsync(filter == null ? QueryFilter.Empty : QueryFilter.Where(filter));
		}

		public virtual async Task<PageResult<TEntity>> GetPageAsync(PageQuery<TEntity> query) {
			ThrowIfDisposed();

			if (!SupportsPaging)
				throw new NotSupportedException("The repository does not support paging");

			try {
				// TODO: log this operation

				return await PageableRepository.GetPageAsync(query, CancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}
	}
}
